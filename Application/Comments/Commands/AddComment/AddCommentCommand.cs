using Application.Comments.DTOs;
using MediatR;

namespace Application.Comments.Commands.AddComment
{
    public record AddCommentCommand(long PostId, CreateCommentRequest Request) : IRequest<CommentDto>;
}
