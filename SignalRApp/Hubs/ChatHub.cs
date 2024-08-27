using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Services.Contracts;
using System.Security.Cryptography.X509Certificates;

namespace SignalRApp.Hubs
{
    public class ChatHub : Hub
    {
        public static List<User> ConnectedUsers = new List<User>();
        private readonly UserManager<User> _userManager;
        public ChatHub(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetNickName()
        {
            var userName = Context.User.Identity.Name;
            var connectionId=Context.ConnectionId;
            var allUsers = await _userManager.Users.ToListAsync();
            var user=await _userManager.FindByNameAsync(userName);
            user.ConnectionId = connectionId;
            _userManager.UpdateAsync(user);
            var otherUsers = allUsers.Where(u => u.UserName != userName).ToList();

            if (userName is not null)
            {
                if (!ConnectedUsers.Any(u => u.UserName == userName))
                {
                    ConnectedUsers.Add(user);
                }
                await Clients.All.SendAsync("clientPage",otherUsers);
            }
        }

    }
}
