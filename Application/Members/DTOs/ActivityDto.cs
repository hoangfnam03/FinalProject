using System;

namespace Application.Members.DTOs
{
    public class ActivityDto
    {
        public long Id { get; set; }
        public string Type { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public long? PostId { get; set; }
        public string? PostTitle { get; set; }
        public long? CommentId { get; set; }
        public int? DeltaScore { get; set; }
        public string? Excerpt { get; set; }
    }
}
