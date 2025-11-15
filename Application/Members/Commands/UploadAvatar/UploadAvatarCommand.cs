using Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Members.Commands.UploadAvatar
{
    public record UploadAvatarCommand(IFormFile File) : IRequest<FileDto>;
}