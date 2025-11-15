using Application.Common.Models;
using Application.Posts.DTOs;
using Application.Search.DTOs;
using Application.Search.Queries;
using Application.Search.Queries.SearchAll;
using Application.Search.Queries.SearchPosts;
using Application.Search.Queries.SearchTags;
using Application.Search.Queries.SearchUsers;
using Application.Tags.DTOs;
using Application.Users.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers
{
    [ApiController]
    [Route("api/v1/search")]
    public class SearchController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SearchController(IMediator mediator) => _mediator = mediator;

        // GET /api/v1/search?q=&type=posts|users|tags|all&page=&pageSize=
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] string type = "all",
                                               [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
                                               [FromQuery] string? sort = null,
                                               CancellationToken ct = default)
        {
            type = (type ?? "all").ToLowerInvariant();

            switch (type)
            {
                case "posts":
                    {
                        var res = await _mediator.Send(new SearchPostsQuery(q, page, pageSize, sort ?? "relevance"), ct);
                        return Ok(res);
                    }
                case "users":
                    {
                        var res = await _mediator.Send(new SearchUsersQuery(q, page, pageSize), ct);
                        return Ok(res);
                    }
                case "tags":
                    {
                        var res = await _mediator.Send(new SearchTagsQuery(q, page, pageSize), ct);
                        return Ok(res);
                    }
                case "all":
                default:
                    {
                        var res = await _mediator.Send(new SearchAllQuery(q, page, pageSize), ct);
                        return Ok(res);
                    }
            }
        }
    }
}
