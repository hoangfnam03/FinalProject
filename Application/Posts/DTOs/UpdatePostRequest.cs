using System.Collections.Generic;

namespace Application.Posts.DTOs
{
    public class UpdatePostRequest
    {
        public string? Title { get; set; }
        public string? Body { get; set; }
        public List<string>? Tags { get; set; }
        public string? CategorySlug { get; set; } // tùy chọn
        public long? CategoryId { get; set; }     // tùy chọn (nếu FE gửi thẳng id)
        public bool? RemoveCategory { get; set; }
    }
}
