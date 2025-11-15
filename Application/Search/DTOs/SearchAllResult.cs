using Application.Common.Models;
using Application.Posts.DTOs;
using Application.Tags.DTOs;
using Application.Users.DTOs;

namespace Application.Search.DTOs
{
    public class SearchAllResult
    {
        public Paged<PostDto> Posts { get; set; } = new();
        public Paged<MemberSummaryDto> Users { get; set; } = new();
        public Paged<TagDto> Tags { get; set; } = new();
    }
}
