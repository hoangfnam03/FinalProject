using Application.Posts.DTOs;
using MediatR;

namespace Application.Posts.Queries.GetPostRevisionDetail
{
    public record GetPostRevisionDetailQuery(long PostId, long RevisionId) : IRequest<RevisionDetailDto>;
}
