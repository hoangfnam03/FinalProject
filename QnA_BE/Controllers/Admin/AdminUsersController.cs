using Application.Admin.Users.Commands;
using Application.Admin.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers.Admin
{
    public class AdminUsersController : AdminBaseController
    {
        private readonly IMediator _mediator;

        public AdminUsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("users")]
        public async Task<IActionResult> List(
            [FromQuery] string? keyword,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            var result = await _mediator.Send(new ListAdminUsersQuery(keyword, page, pageSize), ct);
            return Ok(result);
        }

        [HttpPost("users")]
        public async Task<IActionResult> Create([FromBody] CreateStaffRequest body, CancellationToken ct)
        {
            var dto = await _mediator.Send(new CreateStaffUserCommand(body.FullName, body.Email, body.Password, body.Role), ct);
            return Ok(dto);
        }

        [HttpDelete("users/{memberId:long}")]
        public async Task<IActionResult> Delete(long memberId, CancellationToken ct)
        {
            await _mediator.Send(new DeleteUserCommand(memberId), ct);
            return NoContent();
        }

        public class CreateStaffRequest
        {
            public string FullName { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Password { get; set; } = default!;
            public string Role { get; set; } = "Moderator"; // Moderator | Admin
        }
    }
}
