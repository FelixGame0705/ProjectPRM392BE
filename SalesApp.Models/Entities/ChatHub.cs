
using GoEStores.Core.Base;
using SalesApp.Models.Entities;

namespace GoEStores.Repositories.Entity
{
    public class ChatHub : BaseEntity
    {
        public int? FUserId { get; set; }  // Người khởi tạo chat
        public User FUser { get; set; } = null!; // Ensure FUser is not null

        public int? SUserId { get; set; }  // Người nhận
        public virtual User SUser { get; set; } = null!; // Ensure SUser is not null

        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}
