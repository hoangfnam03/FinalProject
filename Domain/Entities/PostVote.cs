using Domain.Common.Entities;

namespace Domain.Entities
{
    public class PostVote : AuditableEntity
    {
        public long PostId { get; set; }
        public long MemberId { get; set; }
        public int Value { get; set; } // +1 or -1
        public Post? Post { get; set; }
    }
}
