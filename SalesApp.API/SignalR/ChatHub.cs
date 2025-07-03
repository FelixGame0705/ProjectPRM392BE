using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;
using System.Security.Claims;

namespace SalesApp.API.SignalR
{
    public sealed class ChatHub : Hub<IChatClient>
    {
        private readonly INotificationService _notificationService;
        private readonly IChatService _chatService;


        public override async Task OnConnectedAsync()
        {
            await this.Clients.All.RecieveMessage( $"{Context.ConnectionId} has joined.");
        }

        public async Task SendMessage(string message)
        {
            await this.Clients.All.RecieveMessage($"{Context.ConnectionId}:{message}");
        }
        public async Task SendPrivateMessage(string userId, string message)
        {
            await Clients.User(userId).RecieveMessage(message);
        }
        public async Task SendChatMessage(string message)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                await Clients.Caller.RecieveMessage("[Server]: You are not authenticated.");
                return;
            }

            var chatDto = new ChatDto
            {
                UserID = userId.Value,
                Message = message,
                SentAt = DateTime.Now
            };

            await _chatService.SaveChatMessageAsync(chatDto);

            // Gửi cho tất cả client tin nhắn mới
            await Clients.All.RecieveMessage($"[User {userId}]: {message}");
        }

        private int? GetUserId()
        {
            var userIdStr = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out var userId) ? userId : null;
        }



    }
}
