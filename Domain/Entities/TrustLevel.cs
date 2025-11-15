using Domain.Common;
using Domain.Common.Entities;

namespace Domain.Entities
{
    public class TrustLevel : AuditableEntity, ITenantEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid? TenantId { get; set; }
    }
}
