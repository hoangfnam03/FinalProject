using Application.Categories.Queries.GetPostsByCategory;
using Application.Common.Models;
using Application.Posts.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Handlers
{
    public class GetPostsByCategoryQueryHandler : IRequestHandler<GetPostsByCategoryQuery, Paged<PostDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetPostsByCategoryQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<Paged<PostDto>> Handle(GetPostsByCategoryQuery rq, CancellationToken ct)
        {
            var page = rq.Page <= 0 ? 1 : rq.Page;
            var size = rq.PageSize <= 0 ? 20 : rq.PageSize;

            var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Slug == rq.CategorySlug, ct)
                      ?? throw new KeyNotFoundException("Category not found.");

            var posts = _db.Posts
                .Include(p => p.Author)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Where(p => p.CategoryId == cat.Id);

            if (!string.IsNullOrWhiteSpace(rq.Q))
            {
                var key = rq.Q.Trim();
                posts = posts.Where(p => p.Title.Contains(key) || p.Body.Contains(key));
            }

            var total = await posts.CountAsync(ct);

            var scores = _db.PostVotes.GroupBy(v => v.PostId)
                            .Select(g => new { PostId = g.Key, Score = g.Sum(x => x.Value) });

            var joined = from p in posts
                         join s in scores on p.Id equals s.PostId into ps
                         from s in ps.DefaultIfEmpty()
                         select new { p, Score = (int?)s.Score ?? 0 };

            joined = (rq.Sort ?? "created_desc").ToLowerInvariant() switch
            {
                "created_asc" => joined.OrderBy(x => x.p.CreatedAt),
                "score_desc" => joined.OrderByDescending(x => x.Score).ThenByDescending(x => x.p.CreatedAt),
                _ => joined.OrderByDescending(x => x.p.CreatedAt)
            };

            var items = await joined
                .Skip((page - 1) * size).Take(size)
                .Select(x => new PostDto
                {
                    Id = x.p.Id,
                    Title = x.p.Title,
                    AuthorDisplayName = x.p.Author!.DisplayName,
                    CreatedAt = x.p.CreatedAt,
                    Score = x.Score,
                    Tags = x.p.PostTags.Select(pt => pt.Tag!.Name).ToList(),
                    PreviewBody = x.p.Body.Length > 160
                        ? x.p.Body.Substring(0, 160) + "..."
                        : x.p.Body
                })
                .ToListAsync(ct);

            return new Paged<PostDto> { Page = page, PageSize = size, Total = total, Items = items };
        }
    }
}
