using Microsoft.AspNetCore.SignalR;

namespace CVSante.Services
{
    public class NotificationHub : Hub
    {
        public async Task SendNewItemNotification()
        {
            await Clients.All.SendAsync("ReceiveNewItemNotification");
        }
    }
}