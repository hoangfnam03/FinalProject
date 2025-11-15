using System;

namespace Application.Comments.DTOs
{
    public class CommentRevisionDto
    {
        public long Id { get; set; }
        public long CommentId { get; set; }
        public string EditorDisplayName { get; set; } = "unknown";
        public DateTime CreatedAt { get; set; }
        public string? Summary { get; set; }
    }
}
