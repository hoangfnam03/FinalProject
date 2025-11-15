using Application.Common.Models;
using Application.Posts.DTOs;
using MediatR;

namespace Application.Categories.Queries.GetPostsByCategory
{
    public record GetPostsByCategoryQuery(string CategorySlug, int Page = 1, int PageSize = 20, string? Q = null, string? Sort = "created_desc")
        : IRequest<Paged<PostDto>>;
}
