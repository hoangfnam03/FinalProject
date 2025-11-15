using Application.Comments.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Comments.Commands.UploadCommentImage
{
    public record UploadCommentImageCommand(long CommentId, IFormFile File, string? Caption) : IRequest<CommentAttachmentDto>;
}
