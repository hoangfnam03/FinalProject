using Application.Common.Models;
using Application.Tags.DTOs;
using Application.Tags.Queries;
using Application.Tags.Queries.GetPostsByTag;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers
{
    [ApiController]
    [Route("api/v1/tags")]
    public class TagsController : ControllerBase
    {
        private readonly ISender _sender;

        public TagsController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Danh sách tags cho trang /tags (có search + paging)
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(Paged<TagDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Paged<TagDto>>> List([FromQuery] ListTagsQuery query, CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            return Ok(result);
        }

        /// <summary>
        /// Suggest tags cho autocomplete (trang đặt câu hỏi)
        /// </summary>
        [HttpGet("suggest")]
        [ProducesResponseType(typeof(IReadOnlyList<TagDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<TagDto>>> Suggest([FromQuery] SuggestTagsQuery query, CancellationToken ct)
        {
            var result = await _sender.Send(query, ct);
            return Ok(result);
        }

        // GET /api/v1/tags/{slug}/posts?page=1&pageSize=20&q=&sort=
        [HttpGet("{slug}/posts")]
        [ProducesResponseType(typeof(Paged<TagPostDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<Paged<TagPostDto>>> GetPostsByTag(
            [FromRoute] string slug,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? q = null,
            [FromQuery] string? sort = "created_desc",
            CancellationToken ct = default)
        {
            var result = await _sender.Send(new GetPostsByTagQuery(slug, page, pageSize, q, sort), ct);
            return Ok(result);
        }

    }
}
