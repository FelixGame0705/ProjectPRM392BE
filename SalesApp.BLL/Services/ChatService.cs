using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SalesApp.API.Controllers;
using SalesApp.DAL.Repositories;
using SalesApp.DAL.UnitOfWork;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApp.BLL.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUnitOfWork _context;
        private readonly IMapper _mapper;

        public ChatService( IChatRepository chatRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _chatRepository = chatRepository;
            _context = unitOfWork;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ChatDto>> GetChatHistoryAsync(int userId)
        {
            var chats = await _chatRepository
                .GetQueryable()
                .Where(c => c.UserID == userId)
                .OrderByDescending(c => c.SentAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ChatDto>>(chats);
        }


        public async Task SaveChatMessageAsync(ChatDto chat)
        {
            if (chat == null)
            {
                throw new ArgumentNullException(nameof(chat), "chat cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(chat.Message))
            {
                throw new ArgumentException("Chat message cannot be null or empty.", nameof(chat.Message));
            }
            if (chat.UserID <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(chat.UserID));
            }
            var entity = _mapper.Map<ChatMessage>(chat);
            await _chatRepository.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
    }

}
