using Application.Admin.Reports.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Reports.Queries
{
    public class ListPendingReportsQueryHandler : IRequestHandler<ListPendingReportsQuery, Paged<ReportListItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public ListPendingReportsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Paged<ReportListItemDto>> Handle(ListPendingReportsQuery request, CancellationToken ct)
        {
            var baseQuery =
                from r in _db.Reports.AsNoTracking()
                join m in _db.Members.AsNoTracking() on r.ReporterMemberId equals m.Id
                where r.Status == ReportStatus.Pending
                select new
                {
                    Report = r,
                    ReporterName = m.DisplayName
                };

            var total = await baseQuery.CountAsync(ct);

            var rows = await baseQuery
                .OrderByDescending(x => x.Report.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            // Lấy snippet nội dung target (Post/Comment)
            var items = new List<ReportListItemDto>(rows.Count);
            foreach (var x in rows)
            {
                string snippet = "";

                if (x.Report.TargetType == ReportTargetType.Post)
                {
                    var post = await _db.Posts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == x.Report.TargetId, ct);
                    snippet = post?.Title ?? "(Deleted post)";
                }
                else
                {
                    var comment = await _db.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == x.Report.TargetId, ct);
                    snippet = comment?.Body ?? "(Deleted comment)";
                }

                if (snippet.Length > 160) snippet = snippet.Substring(0, 160) + "...";

                items.Add(new ReportListItemDto
                {
                    Id = x.Report.Id,
                    ReporterName = x.ReporterName,
                    Reason = x.Report.Reason,
                    TargetType = x.Report.TargetType.ToString(),
                    TargetId = x.Report.TargetId,
                    TargetContent = snippet,
                    CreatedAt = x.Report.CreatedAt
                });
            }

            return new Paged<ReportListItemDto>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total,
                Items = items
            };
        }
    }
}
