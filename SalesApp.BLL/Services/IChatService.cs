using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApp.BLL.Services
{
    public interface IChatService
    {
        Task SaveChatMessageAsync(ChatDto chat);
        Task<IEnumerable<ChatDto>> GetChatHistoryAsync(int userId);

    }

}
