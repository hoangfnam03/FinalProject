using Application.Stats.DTOs;
using Application.Stats.Queries;
using Application.Stats.Queries.GetMyStats;
using Application.Stats.Queries.GetOveriewStats;
using Application.Stats.Queries.GetUserStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class StatsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public StatsController(IMediator mediator) => _mediator = mediator;

        // GET /api/v1/stats/me (auth)
        [HttpGet("stats/me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UserStatsDto>> GetMyStats(CancellationToken ct)
        {
            var res = await _mediator.Send(new GetMyStatsQuery(), ct);
            return Ok(res);
        }

        // GET /api/v1/users/{id}/stats (public - hiển thị trên trang cá nhân)
        [HttpGet("users/{id:long}/stats")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UserStatsDto>> GetUserStats([FromRoute] long id, CancellationToken ct)
        {
            var res = await _mediator.Send(new GetUserStatsQuery(id), ct);
            return Ok(res);
        }

        // GET /api/v1/stats/overview (public hoặc hạn chế quyền tùy bạn)
        // Nếu muốn hạn chế: thay [AllowAnonymous] => [Authorize(Roles = "Administrator,Moderator")]
        [HttpGet("stats/overview")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OverviewStatsDto>> GetOverview([FromQuery] int top = 10, CancellationToken ct = default)
        {
            var res = await _mediator.Send(new GetOverviewStatsQuery(top), ct);
            return Ok(res);
        }
    }
}
