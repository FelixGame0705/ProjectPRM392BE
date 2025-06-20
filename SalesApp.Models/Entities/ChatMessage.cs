using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesApp.Models.Entities
{
    [Table("ChatMessages")]
    public class ChatMessage
    {
        [Key]
        public int ChatMessageID { get; set; }

        public int? UserID { get; set; }

        public string? Message { get; set; }

        [Required]
        public DateTime SentAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
    }
}