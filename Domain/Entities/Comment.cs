using Domain.Common;
using Domain.Common.Entities;

namespace Domain.Entities
{
    public class Comment : SoftDeletableEntity
    {
        public long PostId { get; set; }
        public long AuthorId { get; set; }   // Member.Id
        public string Body { get; set; } = default!;

        public Post? Post { get; set; }
        public Member? Author { get; set; }
        public long? ParentCommentId { get; set; }
    }
}
