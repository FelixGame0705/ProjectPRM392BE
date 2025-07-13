using GoEStores.Core.Base;
using SalesApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoEStores.Repositories.Entity
{
    public class ChatMessage : BaseEntity
    {
        public string Content { get; set; } = null!;
        public string Type { get; set; } = null!;
        public int? SenderId { get; set; } = null!;
        public virtual User Sender { get; set; } = null!;
        public Guid ChatHubId { get; set; }
        public virtual ChatHub ChatHub { get; set; } = null!;
    }
}
