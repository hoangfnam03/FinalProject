using MediatR;

namespace Application.Posts.Commands.RemoveAttachment
{
    public record RemoveAttachmentCommand(long PostId, long AttachmentId) : IRequest<bool>;
}
