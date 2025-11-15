using Application.Common.Models;
using Application.Posts.DTOs;
using MediatR;

namespace Application.Posts.Queries.ListByTag
{
    public record ListPostsByTagQuery(string TagSlug, int Page = 1, int PageSize = 20)
        : IRequest<Paged<PostDto>>;
}
