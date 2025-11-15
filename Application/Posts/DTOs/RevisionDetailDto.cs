using System;

namespace Application.Posts.DTOs
{
    public class RevisionDetailDto
    {
        public long Id { get; set; }
        public long PostId { get; set; }
        public string EditorDisplayName { get; set; } = "unknown";
        public DateTime CreatedAt { get; set; }
        public string? Summary { get; set; }

        public string? BeforeTitle { get; set; }
        public string? AfterTitle { get; set; }
        public string? BeforeBody { get; set; }
        public string? AfterBody { get; set; }
    }
}
