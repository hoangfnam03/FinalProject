using Application.Admin.Reports.Commands;
using Application.Admin.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers.Admin
{
    public class AdminReportsController : AdminBaseController
    {
        private readonly IMediator _mediator;

        public AdminReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("reports")]
        public async Task<IActionResult> List(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListPendingReportsQuery(page, pageSize), ct);
            return Ok(result);
        }

        [HttpPost("reports/{id:long}/resolve")]
        public async Task<IActionResult> Resolve(long id, [FromBody] ResolveReportRequest body, CancellationToken ct)
        {
            await _mediator.Send(new ResolveReportCommand(id, body.Action), ct);
            return NoContent();
        }

        public class ResolveReportRequest
        {
            public string Action { get; set; } = "Dismiss"; // DeleteContent | Dismiss
        }
    }
}
