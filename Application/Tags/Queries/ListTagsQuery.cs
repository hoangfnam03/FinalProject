using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Tags.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tags.Queries
{
    public sealed record ListTagsQuery : IRequest<Paged<TagDto>>
    {
        public string? Search { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 24;
    }

    public sealed class ListTagsQueryHandler : IRequestHandler<ListTagsQuery, Paged<TagDto>>
    {
        private readonly IApplicationDbContext _context;

        public ListTagsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Paged<TagDto>> Handle(ListTagsQuery request, CancellationToken cancellationToken)
        {
            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 24 : request.PageSize;

            var query = _context.Tags
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var key = request.Search.Trim().ToLower();
                query = query.Where(t =>
                    t.Name.ToLower().Contains(key) ||
                    t.Slug.ToLower().Contains(key));
            }

            var projected = query.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                UsageCount = t.PostTags.Count()
            });

            projected = projected
                .OrderByDescending(x => x.UsageCount)
                .ThenBy(x => x.Name);

            var total = await projected.CountAsync(cancellationToken);

            var items = await projected
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new Paged<TagDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }
    }
}
