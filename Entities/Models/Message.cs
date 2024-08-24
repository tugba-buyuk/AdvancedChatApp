using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class Message
    {
        public int Id { get; set; } //PK
        public string SenderId { get; set; } // FK to User
        public int ChatRoomId { get; set; } // FK to ChatRoom
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public User Sender { get; set; }
        public ChatRoom ChatRoom { get; set; }
        public ICollection<FileAttachment> Attachments { get; set; } // One-to-Many Relationship
    }
}
