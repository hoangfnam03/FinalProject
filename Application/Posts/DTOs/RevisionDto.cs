using System;

namespace Application.Posts.DTOs
{
    public class RevisionDto
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public string EditorDisplayName { get; set; } = "unknown";
        public DateTime CreatedAt { get; set; }
        public string? Summary { get; set; }
    }
}
