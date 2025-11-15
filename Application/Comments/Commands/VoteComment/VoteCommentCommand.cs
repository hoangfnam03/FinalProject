using Application.Comments.DTOs;
using MediatR;

namespace Application.Comments.Commands.VoteComment
{
    public record VoteCommentCommand(long CommentId, int Value) : IRequest<VoteCommentResponse>;
}
