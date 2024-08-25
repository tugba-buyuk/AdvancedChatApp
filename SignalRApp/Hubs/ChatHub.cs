using Microsoft.AspNetCore.SignalR;

namespace SignalRApp.Hubs
{
    public class ChatHub : Hub
    {
        static List<string> clients = new List<string>();
        public override async Task OnConnectedAsync()
        {
            clients.Add(Context.ConnectionId);
            await Clients.All.SendAsync("clients", clients);
            await Clients.Others.SendAsync("userJoined", Context.ConnectionId);
        }
    }
}
