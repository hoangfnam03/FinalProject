using Application.Admin.Dashboard.DTOs;
using MediatR;

namespace Application.Admin.Dashboard.Queries
{
    public record GetAdminDashboardStatsQuery() : IRequest<AdminDashboardStatsDto>;
}
