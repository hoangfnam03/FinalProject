using Application.Posts.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application.Posts.Queries.GetPostAttachments
{
    public record GetPostAttachmentsQuery(long PostId) : IRequest<List<AttachmentDto>>;
}
