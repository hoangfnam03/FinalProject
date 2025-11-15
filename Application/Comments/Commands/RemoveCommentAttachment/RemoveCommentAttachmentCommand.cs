using MediatR;

namespace Application.Comments.Commands.RemoveCommentAttachment
{
    public record RemoveCommentAttachmentCommand(long CommentId, long AttachmentId) : IRequest<bool>;
}
