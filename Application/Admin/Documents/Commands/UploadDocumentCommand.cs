using Application.Admin.Documents.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Admin.Documents.Commands
{
    public record UploadDocumentCommand(IFormFile File) : IRequest<DocumentDto>;
}
