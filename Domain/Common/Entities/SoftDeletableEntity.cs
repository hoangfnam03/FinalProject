using System;

namespace Domain.Common.Entities
{
    public abstract class SoftDeletableEntity : AuditableEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public long? DeletedByMemberId { get; set; }
    }
}
