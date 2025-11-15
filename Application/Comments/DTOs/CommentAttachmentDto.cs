using System;

namespace Application.Comments.DTOs
{
    public class CommentAttachmentDto
    {
        public long Id { get; set; }
        public string Type { get; set; } = "image"; // "image" | "link"
        public DateTime CreatedAt { get; set; }

        // Image
        public string? Url { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public string? Caption { get; set; }

        // Link
        public string? LinkUrl { get; set; }
        public string? DisplayText { get; set; }
    }
}
