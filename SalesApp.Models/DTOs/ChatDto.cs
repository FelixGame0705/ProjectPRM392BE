using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApp.Models.DTOs
{
    public class ChatDto
    {
        public int? UserID { get; set; }

        public string? Message { get; set; }
        [Required]
        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
