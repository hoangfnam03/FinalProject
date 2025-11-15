using Application.Comments.DTOs;
using MediatR;

namespace Application.Comments.Queries.GetCommentRevisionDetail
{
    public record GetCommentRevisionDetailQuery(long CommentId, long RevisionId) : IRequest<CommentRevisionDetailDto>;
}
