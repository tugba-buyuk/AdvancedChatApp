using Entities.Models;
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
                await Clients.Caller.SendAsync("clientPage",otherUsers);
            }
        }

        public async Task SendMessage(string messageContent, string receiverName)
        {
            var userName = Context.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var receiver = await _userManager.Users.FirstAsync(u => u.UserName == receiverName);

            var chatRoom = await _context.ChatRooms
             .Include(cr => cr.Messages)
             .FirstOrDefaultAsync(cr => cr.UserChatRooms.Any(uc =>
                 (uc.UserId == user.Id && uc.ReceiverId == receiver.Id) ||
                 (uc.UserId == receiver.Id && uc.ReceiverId == user.Id)
             ));

            if (chatRoom == null)
            {
                chatRoom = new ChatRoom
                {
                    RoomName = "Private Chat",
                    IsGroup = false,
                    CreatedAt = DateTime.Now,
                    UserChatRooms = new List<UserChatRoom>
                    {
                        new UserChatRoom { UserId = user.Id, JoinedAt = DateTime.Now , ReceiverId=receiver.Id }
                        
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

            var messageDto = new
            {
                SenderId = message.SenderId,
                ChatRoomId = message.ChatRoomId,
                Content = message.Content,
                SentAt = message.SentAt
            };

            _context.Messages.Add(message);
            chatRoom.Messages.Add(message);
            await _context.SaveChangesAsync();

            await Clients.Caller.SendAsync("ReceiveMessage", messageDto, user.UserName);

            if (!string.IsNullOrEmpty(receiver.ConnectionId))
            {
                await Clients.Client(receiver.ConnectionId).SendAsync("ReceiveMessage", messageDto, user.UserName);
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
                    await Clients.Caller.SendAsync("ReceiveMessage", pendingMessage);
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
                user.ConnectionId = null;
                await _userManager.UpdateAsync(user);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task LoadChatHistory(string receiverName)
        {
            var userName = Context.User.Identity.Name;
            var user = await _userManager.FindByNameAsync(userName);
            var receiver = await _userManager.Users.FirstAsync(u => u.UserName == receiverName);

            var chatRoom = await _context.ChatRooms
             .Include(cr => cr.Messages)
             .FirstOrDefaultAsync(cr => cr.UserChatRooms.Any(uc =>
                 (uc.UserId == user.Id && uc.ReceiverId == receiver.Id) ||
                 (uc.UserId == receiver.Id && uc.ReceiverId == user.Id)
             ));

            if (chatRoom != null)
            {
                var chatHistory = chatRoom.Messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        m.SenderId,
                        m.Content,
                        m.SentAt,
                        SenderUserName = m.SenderId == user.Id ? userName : receiverName
                    })
                    .ToList();

                await Clients.Caller.SendAsync("LoadChatHistory", chatHistory);
            }
        }

        // Yardımcı metot dosya türünü belirlemek için
        private string GetFileType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return extension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }


        public async Task SendFileBase64(string fileName, string fileBase64)
        {
            // Base64'ü byte dizisine çevir
            var fileContent = Convert.FromBase64String(fileBase64);

            // Dosyanın sunucudaki yolu (örneğin, wwwroot/uploads içinde saklanacak)
            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            // Dosya yolunun bulunduğu dizinin mevcut olduğundan emin olun
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // Dosya içeriğini fiziksel dosyaya yazma
            await File.WriteAllBytesAsync(filePath, fileContent);


            // Veritabanına dosya bilgilerini kaydet
            var fileAttachment = new FileAttachment
            {
                FileName = fileName,
                FilePath = filePath, // Dosyanın fiziksel yolunu saklamak için
                FileType = GetFileType(fileName),
                FileSize = fileContent.Length,
                UploadedAt = DateTime.Now
            };

            _context.FileAttachments.Add(fileAttachment);
            await _context.SaveChangesAsync();


            // Dosya URL'sini oluştur
            var fileUrl = $"/files/{fileName}";
            var senderName = Context.User.Identity.Name;
            // Dosya URL'sini tüm istemcilere gönder
            await Clients.Caller.SendAsync("ReceiveFile", fileName, fileUrl, senderName);
            await Clients.Caller.SendAsync("ReceiveFile", fileName, fileUrl, senderName);

            _context.FileAttachments.Add(fileAttachment);
            await _context.SaveChangesAsync();



        }
    }
}
