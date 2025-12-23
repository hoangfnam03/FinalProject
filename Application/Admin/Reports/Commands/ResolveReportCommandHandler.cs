using Application.Common.Interfaces;
using Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Reports.Commands
{
    public class ResolveReportCommandHandler : IRequestHandler<ResolveReportCommand>
    {
        private readonly IApplicationDbContext _db;

        public ResolveReportCommandHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Handle(ResolveReportCommand request, CancellationToken ct)
        {
            var report = await _db.Reports.FirstOrDefaultAsync(x => x.Id == request.ReportId, ct);
            if (report == null) return;

            var action = request.Action?.Trim() ?? "Dismiss";

            if (action.Equals("DeleteContent", StringComparison.OrdinalIgnoreCase))
            {
                if (report.TargetType == ReportTargetType.Post)
                {
                    var post = await _db.Posts.FirstOrDefaultAsync(x => x.Id == report.TargetId, ct);
                    if (post != null)
                    {
                        post.IsDeleted = true;
                        post.DeletedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    var comment = await _db.Comments.FirstOrDefaultAsync(x => x.Id == report.TargetId, ct);
                    if (comment != null)
                    {
                        comment.IsDeleted = true;
                        comment.DeletedAt = DateTime.UtcNow;
                    }
                }
            }

            report.Status = ReportStatus.Resolved;
            report.ResolvedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }
    }
}
