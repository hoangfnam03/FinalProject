using Application.Admin.Reports.DTOs;
using Application.Common.Models;
using MediatR;

namespace Application.Admin.Reports.Queries
{
    public record ListPendingReportsQuery(int Page, int PageSize) : IRequest<Paged<ReportListItemDto>>;
}
