using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class FileAttachment
    {
        public int Id { get; set; } //PK
        public int MessageId { get; set; } // FK
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation Property
        public Message Message { get; set; } // One-to-Many Relationship
    }
}
