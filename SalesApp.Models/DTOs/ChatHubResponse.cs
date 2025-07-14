using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoEStores.Core.DTO.Responses
{
    public class ChatHubResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset UpdatedTime { get; set; }
        public int FUserId { get; set; } 
        public int SUserId { get; set; }
        public virtual ICollection<ResponseChatMessage> ChatMessages { get; set; } = new List<ResponseChatMessage>();
    }

    public class ResponseChatMessage
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset UpdatedTime { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public int SenderId { get; set; }
    }
}
