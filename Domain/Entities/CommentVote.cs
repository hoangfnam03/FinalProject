using Domain.Common.Entities;

namespace Domain.Entities
{
    public class CommentVote : AuditableEntity
    {
        public long CommentId { get; set; }
        public long MemberId { get; set; }
        public int Value { get; set; } // +1 or -1

        public Comment? Comment { get; set; }
    }
}
