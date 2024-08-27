using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class PendingMessage
    {
        public int Id { get; set; } // PK
        public string SenderId { get; set; } // FK to User
        public string ReceiverId { get; set; } // FK to User
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}
