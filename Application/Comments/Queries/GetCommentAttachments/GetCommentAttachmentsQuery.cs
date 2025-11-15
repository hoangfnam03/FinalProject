using Application.Comments.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application.Comments.Queries.GetCommentAttachments
{
    public record GetCommentAttachmentsQuery(long CommentId) : IRequest<List<CommentAttachmentDto>>;
}
