using Application.Common.Models;
using Application.Posts.DTOs;
using MediatR;

namespace Application.Search.Queries.SearchPosts
{
    public record SearchPostsQuery(string Q, int Page = 1, int PageSize = 20, string? Sort = "relevance")
        : IRequest<Paged<PostDto>>;
}
