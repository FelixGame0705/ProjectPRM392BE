using System;
using System.Collections.Generic;
using System.Linq;
namespace GoEStores.Core.Base
{
    public abstract class BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedTime = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
            UpdatedTime = CreatedTime;
            Status = BaseEnum.Active.ToString();
        }

        public Guid Id { get; set; }
        public string Status { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset UpdatedTime { get; set; }
    }
}
