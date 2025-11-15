using Application.Comments.DTOs;
using MediatR;

namespace Application.Comments.Commands.AddCommentLink
{
    public record AddCommentLinkCommand(long CommentId, AddCommentLinkRequest Request) : IRequest<CommentAttachmentDto>;
}
