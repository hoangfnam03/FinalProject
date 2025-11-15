using System.Collections.Generic;

namespace Application.Posts.DTOs
{
    public class CreatePostRequest
    {
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public List<string>? Tags { get; set; }
        public string? CategorySlug { get; set; } // tùy chọn
        public long? CategoryId { get; set; }     // tùy chọn (nếu FE gửi thẳng id)

    }
}
