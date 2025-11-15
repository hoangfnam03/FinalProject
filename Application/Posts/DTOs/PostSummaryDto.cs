using System;
using System.Collections.Generic;

namespace Application.Posts.DTOs
{
    public class PostSummaryDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = default!;
        public string AuthorDisplayName { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public string PreviewBody { get; set; } = default!;
    }
}
