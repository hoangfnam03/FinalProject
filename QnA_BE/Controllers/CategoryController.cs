using Application.Categories.Commands.CreateCategory;
using Application.Categories.Commands.UpdateCategory;
using Application.Categories.DTOs;
using Application.Categories.Queries.GetCategory;
using Application.Categories.Queries.GetCategoryBySlug;
using Application.Categories.Queries.GetPostsByCategory;
using Application.Common.Models;
using Application.Posts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace QnA_BE.Controllers
{
    [ApiController]
    [Route("api/v1")]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CategoryController(IMediator mediator) => _mediator = mediator;

        // GET /api/v1/categories?includeHidden=false
        [HttpGet("categories")]
        public async Task<ActionResult<List<CategoryDto>>> GetCategories([FromQuery] bool includeHidden = false, CancellationToken ct = default)
        {
            var data = await _mediator.Send(new GetCategoriesQuery(includeHidden), ct);
            return Ok(data);
        }

        // GET /api/v1/categories/{slug}
        [HttpGet("categories/{slug}")]
        public async Task<ActionResult<CategoryDto>> GetBySlug([FromRoute] string slug, CancellationToken ct = default)
        {
            var data = await _mediator.Send(new GetCategoryBySlugQuery(slug), ct);
            return Ok(data);
        }

        // GET /api/v1/categories/{slug}/posts?page=&pageSize=&q=&sort=
        [HttpGet("categories/{slug}/posts")]
        public async Task<ActionResult<Paged<PostDto>>> GetPosts([FromRoute] string slug, [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20, [FromQuery] string? q = null, [FromQuery] string? sort = "created_desc",
            CancellationToken ct = default)
        {
            var data = await _mediator.Send(new GetPostsByCategoryQuery(slug, page, pageSize, q, sort), ct);
            return Ok(data);
        }

        // POST /api/v1/admin/categories  (admin)
        [HttpPost("admin/categories")]
        [Authorize] // thêm policy Admin nếu có
        public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryCreateUpdateRequest request, CancellationToken ct = default)
        {
            var dto = await _mediator.Send(new CreateCategoryCommand(request), ct);
            return CreatedAtAction(nameof(GetBySlug), new { slug = dto.Slug }, dto);
        }

        // PUT /api/v1/admin/categories/{id} (admin)
        [HttpPut("admin/categories/{id:long}")]
        [Authorize]
        public async Task<ActionResult<CategoryDto>> Update([FromRoute] long id, [FromBody] CategoryCreateUpdateRequest request, CancellationToken ct = default)
        {
            var dto = await _mediator.Send(new UpdateCategoryCommand(id, request), ct);
            return Ok(dto);
        }
    }
}
