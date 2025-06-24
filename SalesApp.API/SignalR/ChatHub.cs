using Microsoft.AspNetCore.SignalR;
using SalesApp.BLL.Services;

namespace SalesApp.API.SignalR
{
    public sealed class ChatHub : Hub<IChatClient>
    {
        private readonly INotificationService _notificationService;

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
      


    }
}
