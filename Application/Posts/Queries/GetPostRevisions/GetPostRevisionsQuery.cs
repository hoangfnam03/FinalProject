using Application.Common.Models;
using Application.Posts.DTOs;
using MediatR;

namespace Application.Posts.Queries.GetPostRevisions
{
    public record GetPostRevisionsQuery(long PostId, int Page = 1, int PageSize = 20) : IRequest<Paged<RevisionDto>>;
}
