using Application.Posts.DTOs;
using MediatR;

namespace Application.Posts.Commands.AddPostLink
{
    public record AddPostLinkCommand(long PostId, AddLinkRequest Request) : IRequest<AttachmentDto>;
}
