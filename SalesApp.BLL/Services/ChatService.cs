using AutoMapper;
using GoEStores.Core.Base;
using GoEStores.Core.DTO.Responses;
using GoEStores.Repositories.Entity;
using GoEStores.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using SalesApp.BLL.HubRealTime;
using SalesApp.BLL.Services;
using SalesApp.DAL.Repositories;
using SalesApp.DAL.UnitOfWork;
using System.Linq;

namespace GoEStores.Services.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHubR> _hubContext;

        public ChatService(IUnitOfWork unitOfWork, IUserService currentUserService, IMapper mapper, IHubContext<ChatHubR> hubContext)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _hubContext = hubContext;
        }

        public async Task<ChatHubResponse> CreateChatHup(int secondUserId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var currentUserId = _currentUserService.GetUserId();

                var checkdupl = await _unitOfWork.Repository<ChatHub>()
                    .GetFirstOrDefaultAsync(_ => (_.FUserId == currentUserId && _.SUserId == secondUserId) ||
                    (_.FUserId == secondUserId && _.SUserId == currentUserId));
                if (checkdupl != null)
                {
                    throw new BaseException(StatusCodes.Status409Conflict, checkdupl.Id.ToString());
                }
                var chatHup = new ChatHub
                {
                    Id = Guid.NewGuid(),
                    FUserId = currentUserId,
                    SUserId = secondUserId,
                    Status = "Active",
                    CreatedTime = DateTimeOffset.Now,
                    UpdatedTime = DateTimeOffset.Now
                };
                await _unitOfWork.Repository<ChatHub>().AddAsync(chatHup);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<ChatHubResponse>(chatHup);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task CreateChatMessage(Guid chatHupId, string content, string type)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var currentUserId = _currentUserService.GetUserId();
                var chatHup = await _unitOfWork.Repository<ChatHub>()
                    .GetFirstOrDefaultAsync(_ => _.Id == chatHupId);
                if (chatHup is null)
                {
                    throw new BaseException(StatusCodes.Status404NotFound, "ChatHub not found");
                }
                if (chatHup.FUserId != currentUserId && chatHup.SUserId != currentUserId)
                {
                    throw new BaseException(StatusCodes.Status403Forbidden, "You are not a member of this chat hub");
                }
                var chatMessage = new ChatMessage
                {
                    ChatHubId = chatHupId,
                    Content = content,
                    Type = type,
                    SenderId = currentUserId
                };
                await _unitOfWork.Repository<ChatMessage>().AddAsync(chatMessage);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var receiverId = chatHup.FUserId == currentUserId ? chatHup.SUserId : chatHup.FUserId;

                await _hubContext.Clients.User(receiverId.ToString())
                                 .SendAsync("ReceiveMessage", new
                                 {
                                     SenderId = currentUserId,
                                     Content = chatMessage.Content,
                                     Type = chatMessage.Type,
                                     SentAt = chatMessage.CreatedTime
                                 });
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<List<ChatHubResponse>> GetAllChatHupsByUserId(int userId)
        {
            try
            {
                var chatHups = await _unitOfWork.Repository<ChatHub>()
                                                .GetAllAsync(_ => _.FUserId == userId || _.SUserId == userId, null);
                if (chatHups is null)
                {
                    throw new BaseException(StatusCodes.Status404NotFound, "No chat hubs found for this user.");
                }
                chatHups = chatHups.OrderByDescending(_ => _.CreatedTime).ToList();
                var responseChatHups = _mapper.Map<List<ChatHubResponse>>(chatHups);
                return responseChatHups;
            }
            catch
            {
                throw;
            }
        }

        public async Task<ChatHubResponse> GetChatHupById(Guid chatHupId)
        {
            try
            {
                var chatHup = await _unitOfWork.Repository<ChatHub>()
                    .GetFirstOrDefaultAsync(
                            predicate: _ => _.Id == chatHupId,
                            includeProperties: "ChatMessages");
                if (chatHup is null)
                {
                    throw new BaseException(StatusCodes.Status404NotFound, "ChatHub not found");
                }
                chatHup.ChatMessages = chatHup.ChatMessages
                                              .OrderByDescending(_ => _.CreatedTime)
                                              .ToList();
                return _mapper.Map<ChatHubResponse>(chatHup);
            }
            catch
            {
                throw;
            }
        }
    }
}