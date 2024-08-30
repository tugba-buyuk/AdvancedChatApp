using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class FileAttachment
    {
        public int Id { get; set; } // Primary Key
        public int MessageId { get; set; } // Foreign Key to Message
        public string FileName { get; set; } = string.Empty; // Dosya adı
        public string FileExtension { get; set; } = string.Empty; // Dosya uzantısı (örneğin, .jpg, .pdf)
        public string FilePath { get; set; }=string.Empty;
        public long FileSize { get; set; } // Dosya boyutu (byte cinsinden)
        public string FileType { get; set; } = string.Empty; // MIME tipi (örneğin, image/jpeg)
        public DateTime UploadedAt { get; set; } = DateTime.Now; // Dosyanın yüklendiği tarih

        // Navigation Property
        public Message Message { get; set; } // One-to-Many Relationship
    }
}
