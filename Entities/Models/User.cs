using Microsoft.AspNetCore.Identity;

namespace Entities.Models
{
    public class User : IdentityUser
    {
        public DateTime LastLogin { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? ConnectionId { get; set; }
        public string ProfileImage {  get; set; } = string.Empty;

        public ICollection<Message> SentMessages { get; set; } // Gönderilen mesajlar
        public ICollection<PrivateMessage> SentPrivateMessages { get; set; } // Gönderilen özel mesajlar
        public ICollection<PrivateMessage> ReceivedPrivateMessages { get; set; } // Alınan özel mesajlar
        public ICollection<UserChatRoom> UserChatRooms { get; set; } // Kullanıcının chat odaları
        public ICollection<TypingStatus> TypingStatuses { get; set; } // Kullanıcının yazma durumları
    }

}
