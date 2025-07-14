using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;


namespace SalesApp.BLL.HubRealTime
{

    
        public class NotificationHub : Hub
        {
            public async Task SendNotification(string userId, string message)
            {
                await Clients.User(userId).SendAsync("ReceiveNotification", message);
            }
        }
    
}
