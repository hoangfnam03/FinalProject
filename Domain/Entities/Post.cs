using Domain.Common;
using Domain.Common.Entities;

namespace Domain.Entities
{
    public class Post : SoftDeletableEntity, ITenantEntity
    {
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public long AuthorId { get; set; }            // Member.Id
        public Guid? TenantId { get; set; }

        public Member? Author { get; set; }
        public ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
        public long? CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
