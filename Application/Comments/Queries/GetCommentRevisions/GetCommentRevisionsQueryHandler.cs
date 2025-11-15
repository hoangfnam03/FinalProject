using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Comments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Application.Comments.Queries.GetCommentRevisions
{
    public class GetCommentRevisionsQueryHandler : IRequestHandler<GetCommentRevisionsQuery, Paged<CommentRevisionDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetCommentRevisionsQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<Paged<CommentRevisionDto>> Handle(GetCommentRevisionsQuery request, CancellationToken ct)
        {
            var q = _db.CommentRevisions.AsNoTracking()
                        .Where(r => r.CommentId == request.CommentId)
                        .OrderByDescending(r => r.CreatedAt);

            var total = await q.CountAsync(ct);

            var items = await q
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(r => new CommentRevisionDto
                {
                    Id = r.Id,
                    CommentId = r.CommentId,
                    EditorDisplayName = r.Editor != null ? r.Editor.DisplayName : "unknown",
                    CreatedAt = r.CreatedAt,
                    Summary = r.Summary
                })
                .ToListAsync(ct);

            return new Paged<CommentRevisionDto>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total,
                Items = items
            };
        }
    }
}
