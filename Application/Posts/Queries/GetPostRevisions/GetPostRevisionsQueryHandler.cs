// Application/Posts/Queries/GetPostRevisions/GetPostRevisionsQueryHandler.cs
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Posts.DTOs;
using Application.Posts.Queries.GetPostRevisions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.Posts.Queries.GetPostRevisions
{
    public class GetPostRevisionsQueryHandler : IRequestHandler<GetPostRevisionsQuery, Paged<RevisionDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetPostRevisionsQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<Paged<RevisionDto>> Handle(GetPostRevisionsQuery request, CancellationToken ct)
        {
            var q = _db.PostRevisions.AsNoTracking()
                        .Where(r => r.PostId == request.PostId)
                        .OrderByDescending(r => r.CreatedAt);

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new RevisionDto
                {
                    Id = r.Id,
                    PostId = r.PostId,
                    EditorDisplayName = r.Editor != null ? r.Editor.DisplayName : "unknown",
                    CreatedAt = r.CreatedAt,
                    Summary = r.Summary
                })
                .ToListAsync(ct);

            return new Paged<RevisionDto>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total,
                Items = items
            };
        }
    }
}
