
using GoEStores.Core.Base;
using SalesApp.Models.Entities;

namespace GoEStores.Repositories.Entity
{
    public class ChatHub : BaseEntity
    {
        public Guid FUserId { get; set; }  // Người khởi tạo chat
        public User FUser { get; set; }

        public Guid SUserId { get; set; }  // Người nhận
        public virtual User SUser { get; set; }

        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}
