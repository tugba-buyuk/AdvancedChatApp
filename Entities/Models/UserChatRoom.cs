using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class UserChatRoom
    {
        public int Id { get; set; } //PK
        public string UserId { get; set; } //FK 
        public string ReceiverId {  get; set; }
        public int ChatRoomId { get; set; } //FK
        public DateTime JoinedAt { get; set; } = DateTime.Now;

        public User User { get; set; } // Navigation property
        public User Receiver { get; set; } // Navigation property
        public ChatRoom ChatRoom { get; set; } // Navigation property
    }

}
