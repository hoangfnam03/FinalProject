using Application.Auth.Commands.Login;
using Application.Auth.Commands.Register;
using Application.Auth.Commands.VerifyEmail;
using Application.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator) => _mediator = mediator;

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        {
            await _mediator.Send(new RegisterCommand(request), ct);
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken ct)
        {
            await _mediator.Send(new VerifyEmailCommand(request), ct);
            return NoContent();
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var res = await _mediator.Send(new LoginCommand(request), ct);
            return Ok(res);
        }
    }
}
