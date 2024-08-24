using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class ChatRoom
    {
        public int Id { get; set; } //PK
        public string RoomName { get; set; } = string.Empty;
        public bool IsGroup { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public ICollection<UserChatRoom> UserChatRooms { get; set; } // Many-to-Many Relationship
        public ICollection<Message> Messages { get; set; } // One-to-Many Relationship
        public ICollection<TypingStatus> TypingStatuses { get; set; } // One-to-Many Relationship
    }
}
