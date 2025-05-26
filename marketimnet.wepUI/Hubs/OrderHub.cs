using Microsoft.AspNetCore.SignalR;
using marketimnet.Core.Entities;

namespace marketimnet.wepUI.Hubs
{
    public class OrderHub : Hub
    {
        public async Task NewOrder(Order order)
        {
            await Clients.All.SendAsync("ReceiveNewOrder", order);
        }
    }
} 