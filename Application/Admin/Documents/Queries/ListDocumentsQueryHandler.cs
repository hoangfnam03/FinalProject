using Application.Admin.Documents.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Documents.Queries
{
    public class ListDocumentsQueryHandler : IRequestHandler<ListDocumentsQuery, Paged<DocumentDto>>
    {
        private readonly IApplicationDbContext _db;

        public ListDocumentsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Paged<DocumentDto>> Handle(ListDocumentsQuery request, CancellationToken ct)
        {
            var query = _db.Documents.AsNoTracking().OrderByDescending(x => x.UploadedAt);

            var total = await query.CountAsync(ct);

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new DocumentDto
                {
                    Id = x.Id,
                    FileName = x.FileName,
                    FileSize = x.FileSize,
                    UploadedAt = x.UploadedAt
                })
                .ToListAsync(ct);

            return new Paged<DocumentDto>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total,
                Items = items
            };
        }
    }
}
