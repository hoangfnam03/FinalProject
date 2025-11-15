using Application.Common.Models;
using Application.Notifications.Commands.MarkRead;
using Application.Notifications.DTOs;
using Application.Notifications.Queries.GetUnreadCount;
using Application.Notifications.Queries.ListNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public NotificationsController(IMediator mediator) => _mediator = mediator;

        // GET /api/v1/notifications?page=&pageSize=
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Paged<NotificationDto>>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var res = await _mediator.Send(new ListNotificationsQuery(page, pageSize), ct);
            return Ok(res);
        }

        // POST /api/v1/notifications/mark-read
        // body: { "ids": [1,2,3] }
        public class MarkReadRequest { public List<long> Ids { get; set; } = new(); }

        [HttpPost("mark-read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> MarkRead([FromBody] MarkReadRequest rq, CancellationToken ct)
        {
            await _mediator.Send(new MarkReadNotificationsCommand(rq.Ids), ct);
            return NoContent();
        }

        // GET /api/v1/notifications/unread-count
        [HttpGet("unread-count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> UnreadCount(CancellationToken ct)
        {
            var count = await _mediator.Send(new GetUnreadCountQuery(), ct);
            return Ok(new { count });
        }
    }
}
