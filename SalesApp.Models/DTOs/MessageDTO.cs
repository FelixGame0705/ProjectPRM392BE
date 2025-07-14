using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApp.Models.DTOs
{
    public class MessageDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Username { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
