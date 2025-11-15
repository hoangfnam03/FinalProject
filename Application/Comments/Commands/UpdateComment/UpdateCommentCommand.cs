using Application.Comments.DTOs;
using Application.Posts.DTOs;
using MediatR;

namespace Application.Comments.Commands.UpdateComment
{
    public record UpdateCommentCommand(long CommentId, UpdateCommentRequest Request) : IRequest<CommentDto>;
}
