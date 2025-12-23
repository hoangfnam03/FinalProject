using Application.Common.Interfaces;
using Application.Tags.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Tags.Queries
{
    public sealed record SuggestTagsQuery : IRequest<List<TagDto>>
    {
        public string? Q { get; init; }
        public int Limit { get; init; } = 10;
    }

    public sealed class SuggestTagsQueryHandler : IRequestHandler<SuggestTagsQuery, List<TagDto>>
    {
        private readonly IApplicationDbContext _context;

        public SuggestTagsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TagDto>> Handle(SuggestTagsQuery request, CancellationToken cancellationToken)
        {
            var limit = request.Limit is <= 0 or > 50 ? 10 : request.Limit;

            var query = _context.Tags.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Q))
            {
                var key = request.Q.Trim().ToLower();
                query = query.Where(t =>
                    t.Name.ToLower().Contains(key) ||
                    t.Slug.ToLower().Contains(key));
            }

            var items = await query
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Slug = t.Slug,
                    UsageCount = t.PostTags.Count()
                })
                .OrderByDescending(x => x.UsageCount)
                .ThenBy(x => x.Name)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return items;
        }
    }
}
