using Application.Admin.Questions.Commands;
using Application.Admin.Questions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers.Admin
{
    public class AdminPostsController : AdminBaseController
    {
        private readonly IMediator _mediator;

        public AdminPostsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("posts")]
        public async Task<IActionResult> List(
            [FromQuery] string? keyword,
            [FromQuery] string? status,   // All | Answered | Unanswered
            [FromQuery] long? categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {

            var result = await _mediator.Send(new ListAdminPostsQuery(
                keyword, status, categoryId, page, pageSize
            ), ct);

            return Ok(result);
        }

        [HttpDelete("posts/{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
        {
            await _mediator.Send(new DeletePostCommand(id), ct);
            return NoContent();
        }
    }
}
