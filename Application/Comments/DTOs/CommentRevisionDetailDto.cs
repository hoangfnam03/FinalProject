using System;

namespace Application.Comments.DTOs
{
    public class CommentRevisionDetailDto
    {
        public long Id { get; set; }
        public long CommentId { get; set; }
        public string EditorDisplayName { get; set; } = "unknown";
        public DateTime CreatedAt { get; set; }
        public string? Summary { get; set; }
        public string? BeforeBody { get; set; }
        public string? AfterBody { get; set; }
    }
}
