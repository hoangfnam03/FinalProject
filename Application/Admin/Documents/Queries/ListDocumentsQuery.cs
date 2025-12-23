using Application.Admin.Documents.DTOs;
using Application.Common.Models;
using MediatR;

namespace Application.Admin.Documents.Queries
{
    public record ListDocumentsQuery(int Page, int PageSize) : IRequest<Paged<DocumentDto>>;
}
