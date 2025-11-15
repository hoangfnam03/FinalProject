using Application.Common.Models;
using Application.Comments.DTOs;
using MediatR;

namespace Application.Comments.Queries.GetCommentRevisions
{
    public record GetCommentRevisionsQuery(long CommentId, int Page = 1, int PageSize = 20) : IRequest<Paged<CommentRevisionDto>>;
}
