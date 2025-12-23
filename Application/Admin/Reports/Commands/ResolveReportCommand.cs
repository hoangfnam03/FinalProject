using MediatR;

namespace Application.Admin.Reports.Commands
{
    public record ResolveReportCommand(long ReportId, string Action) : IRequest; // DeleteContent | Dismiss
}
