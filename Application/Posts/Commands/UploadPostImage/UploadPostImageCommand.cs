using Application.Common.Models;
using Application.Posts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Posts.Commands.UploadPostImage
{
    public record UploadPostImageCommand(long PostId, IFormFile File, string? Caption) : IRequest<AttachmentDto>;
}
