using Entities.DataSources;
using Entities.Models;
using Microsoft.AspNetCore.SignalR;

namespace SignalRApp.Hubs
{
    public class ChatHub : Hub
    {
        public async Task GetNickName(string nickName)
        {
            var client = new Clients
            {
                ConnectionId = Context.ConnectionId,
                NickName = nickName
            };
            ClientSource.Clients.Add(client);
            await Clients.All.SendAsync("clientList", ClientSource.Clients);

        }

    }
}
