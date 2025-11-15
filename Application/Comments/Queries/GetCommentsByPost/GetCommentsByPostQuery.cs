using Application.Comments.DTOs;
using Application.Common.Models;
using MediatR;

namespace Application.Comments.Queries.GetCommentsByPost
{
    public record GetCommentsByPostQuery(long PostId, int Page = 1, int PageSize = 20)
        : IRequest<Paged<CommentDto>>;
}
