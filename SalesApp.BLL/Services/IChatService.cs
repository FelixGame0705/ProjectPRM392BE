
using GoEStores.Core.DTO.Responses;

namespace GoEStores.Services.Interface
{
    public interface IChatService
    {

        Task<ChatHubResponse> CreateChatHup(Guid SecondUserId);
        Task<List<ChatHubResponse>> GetAllChatHupsByUserId(Guid userId);
        Task<ChatHubResponse> GetChatHupById(Guid chatHupId);
        Task CreateChatMessage(Guid chatHupId, string content, string type);
        //Task DeleteChatMessage(Guid chatMessageId);
        //Task<ResponseChatMessage> UpdateChatMessage(Guid chatMessageId, string content);
    }
}
