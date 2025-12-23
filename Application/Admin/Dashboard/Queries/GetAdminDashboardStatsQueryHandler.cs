using Application.Admin.Dashboard.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Dashboard.Queries
{
    public class GetAdminDashboardStatsQueryHandler : IRequestHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsDto>
    {
        private readonly IApplicationDbContext _db;

        public GetAdminDashboardStatsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<AdminDashboardStatsDto> Handle(GetAdminDashboardStatsQuery request, CancellationToken ct)
        {
            var totalMembers = await _db.Members.CountAsync(ct);
            var totalQuestions = await _db.Posts.CountAsync(ct);

            // Reports entity mới
            var totalReportsPending = await _db.Reports.CountAsync(x => x.Status == Domain.Common.Enums.ReportStatus.Pending, ct);

            return new AdminDashboardStatsDto
            {
                TotalMembers = totalMembers,
                TotalQuestions = totalQuestions,
                TotalReports = totalReportsPending
            };
        }
    }
}
