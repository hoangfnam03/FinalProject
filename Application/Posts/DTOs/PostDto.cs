using System;
using System.Collections.Generic;

namespace Application.Posts.DTOs
{
    public class PostDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = default!;
        public string AuthorDisplayName { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public int Score { get; set; }
        public List<string> Tags { get; set; } = new();
        public string PreviewBody { get; set; } = default!;
        public long? CategoryId { get; init; }
        public string? CategorySlug { get; init; }
        public string? CategoryName { get; init; }
    }
}
