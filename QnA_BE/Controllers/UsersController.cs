using Application.Common.Models;
using Application.Members.DTOs;
using Application.Members.Queries.GetPublicUser;
using Application.Members.Queries.GetUserActivities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UsersController(IMediator mediator) => _mediator = mediator;

        // GET /api/v1/users/{id}
        [HttpGet("{id:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PublicUserDto>> Get(long id, CancellationToken ct)
            => Ok(await _mediator.Send(new GetPublicUserQuery(id), ct));

        // GET /api/v1/users/{id}/activities?page=&pageSize=
        [HttpGet("{id:long}/activities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Paged<ActivityDto>>> Activities(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => Ok(await _mediator.Send(new GetUserActivitiesQuery(id, page, pageSize), ct));
    }
}
