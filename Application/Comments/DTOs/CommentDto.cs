using System;

namespace Application.Comments.DTOs
{
    public class CommentDto
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public string Body { get; set; } = default!;
        public string AuthorDisplayName { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
