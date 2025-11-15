using Domain.Common;
using Domain.Common.Entities;

namespace Domain.Entities
{
    public class Tag : AuditableEntity, ITenantEntity
    {
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public Guid? TenantId { get; set; }

        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
    }
}
