using Microsoft.AspNetCore.SignalR;

namespace marketimnet.wepUI.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendCartNotification(string message, int cartCount)
        {
            await Clients.All.SendAsync("ReceiveCartNotification", message, cartCount);
        }

        public async Task SendOrderNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveOrderNotification", message);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
} 