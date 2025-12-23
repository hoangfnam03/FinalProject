using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Tags.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tags.Queries.GetPostsByTag
{
    public sealed record GetPostsByTagQuery(
        string Slug,
        int Page = 1,
        int PageSize = 20,
        string? Q = null,
        string? Sort = "created_desc"
    ) : IRequest<Paged<TagPostDto>>;

    public sealed class GetPostsByTagQueryHandler : IRequestHandler<GetPostsByTagQuery, Paged<TagPostDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPostsByTagQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Paged<TagPostDto>> Handle(GetPostsByTagQuery request, CancellationToken ct)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

            var slug = (request.Slug ?? "").Trim().ToLower();
            if (string.IsNullOrWhiteSpace(slug))
            {
                return new Paged<TagPostDto> { Page = page, PageSize = pageSize, Total = 0, Items = new() };
            }

            // TagId theo slug
            var tagId = await _context.Tags.AsNoTracking()
                .Where(t => t.Slug.ToLower() == slug)
                .Select(t => t.Id)
                .FirstOrDefaultAsync(ct);

            if (tagId == 0)
            {
                return new Paged<TagPostDto> { Page = page, PageSize = pageSize, Total = 0, Items = new() };
            }

            // NOTE: giả định Post có Title và navigation PostTags
            var postsQuery = _context.Posts.AsNoTracking()
                .Where(p => p.PostTags.Any(pt => pt.TagId == tagId))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var q = request.Q.Trim().ToLower();
                postsQuery = postsQuery.Where(p => p.Title.ToLower().Contains(q));
            }

            postsQuery = (request.Sort ?? "created_desc").ToLower() switch
            {
                "created_asc" => postsQuery.OrderBy(p => p.Id),
                _ => postsQuery.OrderByDescending(p => p.Id)
            };

            var total = await postsQuery.CountAsync(ct);

            var items = await postsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new TagPostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Tags = p.PostTags.Select(pt => pt.Tag.Slug).ToList()
                })
                .ToListAsync(ct);

            return new Paged<TagPostDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }
    }
}
