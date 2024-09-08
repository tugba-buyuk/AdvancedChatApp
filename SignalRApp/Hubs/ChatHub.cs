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
        public ChatHub(UserManager<User> userManager, RepositoryContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task GetNickName()
        {
            var userName = Context.User.Identity.Name;
            var connectionId = Context.ConnectionId;
            var allUsers = await _userManager.Users.ToListAsync();
            var user = await _userManager.FindByNameAsync(userName);
            user.ConnectionId = connectionId;
            await _userManager.UpdateAsync(user);
            var otherUsers = allUsers.Where(u => u.UserName != userName).ToList();

            if (userName is not null)
            {
                if (!ConnectedUsers.Any(u => u.UserName == userName))
                {
                    ConnectedUsers.Add(user);
                }
                await Clients.Caller.SendAsync("clientPage", otherUsers);
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
            var user = await _userManager.GetUserAsync(Context.User);
            if (user != null)
            {
                user.ConnectionId = Context.ConnectionId;
                // Kullanıcı çevrimiçi olduğunda bekleyen mesajları gönder
                var pendingMessages = await _context.PendingMessages
                    .Where(pm => pm.ReceiverId == user.Id)
                    .ToListAsync();

                if (pendingMessages.Any())
                {
                    // `SenderId` ye göre yeşil yuvarlakla unread count güncelle
                    foreach (var pendingMessage in pendingMessages)
                    {
                        var sender = await _context.Users.FindAsync(pendingMessage.SenderId);
                        if (sender != null)
                        {
                            await Clients.Caller.SendAsync("UpdateUnreadMessageCount", sender.UserName);
                        }
                        _context.PendingMessages.Remove(pendingMessage);
                    }
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
                    .ThenInclude(m => m.Attachments) // Dosya eklerini de dahil et
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
                        SenderUserName = m.SenderId == user.Id ? userName : receiverName,
                        Attachments = m.Attachments.Select(a => new
                        {
                            a.FileName,
                            a.FileType,
                            FileUrl = $"/uploads/{a.FileName}" // Dosya URL'sini oluştur
                        }).ToList()
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
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }

        public async Task SendFileBase64(string fileName, string fileBase64, string receiverName)
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

            }

            // Base64'ü byte dizisine çevir
            var fileContent = Convert.FromBase64String(fileBase64);

            // Dosyanın sunucudaki yolu (örneğin, wwwroot/uploads içinde saklanacak)
            var uploadsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            // Dosya yolunun bulunduğu dizinin mevcut olduğundan emin olun
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            // Dosya içeriğini fiziksel dosyaya yazma
            await File.WriteAllBytesAsync(filePath, fileContent);


            var message = new Message
            {
                SenderId = user.Id,
                ChatRoomId = chatRoom.Id,
                SentAt = DateTime.Now,
                Content = $"File: {fileName}", // Mesaj içeriği olarak dosya ismi
                Attachments = new List<FileAttachment>()
            };

            // Veritabanına dosya bilgilerini kaydet
            var fileAttachment = new FileAttachment
            {
                FileName = fileName,
                FilePath = filePath,
                FileExtension = Path.GetExtension(fileName),
                FileType = GetFileType(fileName),
                FileSize = fileContent.Length,
                UploadedAt = DateTime.Now,
                Message = message
            };

            message.Attachments.Add(fileAttachment);
            _context.Messages.Add(message);
            _context.FileAttachments.Add(fileAttachment);

            // Dosya URL'sini oluştur
            var fileUrl = $"/uploads/{fileName}";
            var senderName = Context.User.Identity.Name;
            // Dosya URL'sini tüm istemcilere gönder
            await Clients.Caller.SendAsync("ReceiveFile", fileName, fileUrl, senderName);
            await Clients.Client(receiver.ConnectionId).SendAsync("ReceiveFile", fileName, fileUrl, user.UserName);
            await _context.SaveChangesAsync();

        }
    }
}
