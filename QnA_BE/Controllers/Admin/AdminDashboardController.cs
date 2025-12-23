using Application.Admin.Dashboard.Queries;
using Domain.Common.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers.Admin
{
    public class AdminDashboardController : AdminBaseController
    {
        private readonly IMediator _mediator;

        public AdminDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("dashboard/stats")]
        [Authorize(Policy = "CanViewStats")]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            var dto = await _mediator.Send(new GetAdminDashboardStatsQuery(), ct);
            return Ok(dto);
        }
    }
}
