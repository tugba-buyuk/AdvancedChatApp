﻿using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services.Contracts;
using System.Security.Cryptography.X509Certificates;

namespace SignalRApp.Hubs
{
    public class ChatHub : Hub
    {
        public static List<User> ConnectedUsers = new List<User>();
        private readonly UserManager<User> _userManager;
        private readonly RepositoryContext _context;
        public ChatHub(UserManager<User> userManager,RepositoryContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task GetNickName()
        {
            var userName = Context.User.Identity.Name;
            var connectionId=Context.ConnectionId;
            var allUsers = await _userManager.Users.ToListAsync();
            var user=await _userManager.FindByNameAsync(userName);
            user.ConnectionId = connectionId;
            await _userManager.UpdateAsync(user);
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

        public async Task SendMessage(string messageContent, string receiverName)
        {
            var userName = Context.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var receiver = await _userManager.Users.FirstAsync(u => u.UserName == receiverName);

            var chatRoom = await _context.ChatRooms
                .Include(cr => cr.UserChatRooms)
                .FirstOrDefaultAsync(cr => cr.UserChatRooms.Any(uc => uc.UserId == user.Id && uc.UserId == receiver.Id));

            if (chatRoom == null)
            {
                chatRoom = new ChatRoom
                {
                    RoomName = "Private Chat",
                    IsGroup = false,
                    CreatedAt = DateTime.Now,
                    UserChatRooms = new List<UserChatRoom>
            {
                new UserChatRoom { UserId = user.Id, JoinedAt = DateTime.Now },
                new UserChatRoom { UserId = receiver.Id, JoinedAt = DateTime.Now }
            }
                };

                _context.ChatRooms.Add(chatRoom);
                await _context.SaveChangesAsync();
            }

            var message = new Message
            {
                SenderId = user.Id,
                ChatRoomId = chatRoom.Id,
                Content = messageContent,
                SentAt = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(receiver.ConnectionId))
            {
                await Clients.Client(receiver.ConnectionId).SendAsync("receiveMessage", message);
            }
            else
            {
                // Mesajı PendingMessages tablosuna ekleyin
                var pendingMessage = new PendingMessage
                {
                    SenderId = user.Id,
                    ReceiverId = receiver.Id,
                    Content = messageContent,
                    SentAt = DateTime.Now
                };

                _context.PendingMessages.Add(pendingMessage);
                await _context.SaveChangesAsync();
            }
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.ConnectionId = Context.ConnectionId;
                await _context.SaveChangesAsync();

                // Kullanıcı çevrimiçi olduğunda bekleyen mesajları gönder
                var pendingMessages = await _context.PendingMessages
                    .Where(pm => pm.ReceiverId == userId)
                    .ToListAsync();

                foreach (var pendingMessage in pendingMessages)
                {
                    await Clients.Caller.SendAsync("receiveMessage", pendingMessage);
                    _context.PendingMessages.Remove(pendingMessage);
                }

                await _context.SaveChangesAsync();
            }

            await base.OnConnectedAsync();
        }

         public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                ConnectedUsers.Remove(user);
                await _userManager.UpdateAsync(user);
            }

            await base.OnDisconnectedAsync(exception);
        }

    }
}
