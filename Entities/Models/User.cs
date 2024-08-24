using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class User : IdentityUser
    {
        public DateTime LastLogin { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Message> SentMessages { get; set; } // Gönderilen mesajlar
        public ICollection<PrivateMessage> SentPrivateMessages { get; set; } // Gönderilen özel mesajlar
        public ICollection<PrivateMessage> ReceivedPrivateMessages { get; set; } // Alınan özel mesajlar
        public ICollection<UserChatRoom> UserChatRooms { get; set; } // Kullanıcının chat odaları
        public ICollection<TypingStatus> TypingStatuses { get; set; } // Kullanıcının yazma durumları
    }

}
