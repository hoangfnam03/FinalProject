using System;

namespace Domain.Common.Entities
{
    public abstract class AuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; }
        public long? CreatedByMemberId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public long? LastModifiedByMemberId { get; set; }
    }
}
