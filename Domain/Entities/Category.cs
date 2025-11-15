using Domain.Common.Entities;

namespace Domain.Entities
{
    public class Category : AuditableEntity
    {
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public long? ParentId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsHidden { get; set; }

        public Category? Parent { get; set; }
    }
}
