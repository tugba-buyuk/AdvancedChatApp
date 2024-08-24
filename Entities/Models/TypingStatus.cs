using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class TypingStatus
    {
        public int Id { get; set; } //PK
        public string UserId { get; set; } // FK to User
        public int ChatRoomId { get; set; } // FK to ChatRoom
        public bool IsTyping { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation Properties
        public User User { get; set; }
        public ChatRoom ChatRoom { get; set; }
    }
}
