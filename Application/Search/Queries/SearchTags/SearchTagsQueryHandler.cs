using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Search.Queries;
using Application.Search.Queries.SearchTags;
using Application.Tags.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Search.Queries.SearchTags
{
    public class SearchTagsQueryHandler : IRequestHandler<SearchTagsQuery, Paged<TagDto>>
    {
        private readonly IApplicationDbContext _db;
        public SearchTagsQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<Paged<TagDto>> Handle(SearchTagsQuery rq, CancellationToken ct)
        {
            var page = rq.Page <= 0 ? 1 : rq.Page;
            var size = rq.PageSize <= 0 ? 20 : rq.PageSize;

            var q = (rq.Q ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return new Paged<TagDto> { Page = page, PageSize = size, Total = 0, Items = new List<TagDto>() };

            var tags = _db.Tags
                .AsNoTracking()
                .Where(t => t.Name.Contains(q) || t.Slug.Contains(q));

            // Popularity by usage count
            var usage = _db.PostTags
                .GroupBy(pt => pt.TagId)
                .Select(g => new { TagId = g.Key, Cnt = g.Count() });

            var joined = from t in tags
                         join u in usage on t.Id equals u.TagId into tu
                         from u in tu.DefaultIfEmpty()
                         select new
                         {
                             t.Id,
                             t.Name,
                             t.Slug,
                             Usage = (int?)u.Cnt ?? 0,
                             NameStarts = t.Name.StartsWith(q) || t.Slug.StartsWith(q)
                         };

            var ordered = joined
                .OrderByDescending(x => x.NameStarts)
                .ThenByDescending(x => x.Usage)
                .ThenBy(x => x.Name);

            var total = await tags.CountAsync(ct);

            var items = await ordered
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new TagDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    UsageCount = x.Usage
                })
                .ToListAsync(ct);

            return new Paged<TagDto> { Page = page, PageSize = size, Total = total, Items = items };
        }
    }
}
