using Application.Common.Models;
using Application.Posts.DTOs;
using Application.Posts.Queries.ListPosts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Posts.Handlers
{
    public class ListPostsQueryHandler : IRequestHandler<ListPostsQuery, Paged<PostDto>>
    {
        private readonly Application.Common.Interfaces.IApplicationDbContext _db;
        public ListPostsQueryHandler(Application.Common.Interfaces.IApplicationDbContext db) { _db = db; }

        public async Task<Paged<PostDto>> Handle(ListPostsQuery rq, CancellationToken ct)
        {
            var page = rq.Page <= 0 ? 1 : rq.Page;
            var size = rq.PageSize <= 0 ? 20 : rq.PageSize;

            var query = _db.Posts
                .Include(p => p.Author)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(rq.Q))
            {
                var key = rq.Q.Trim();
                query = query.Where(p => p.Title.Contains(key) || p.Body.Contains(key));
            }

            // Preload scores via group-join to avoid N+1
            var scores = _db.PostVotes
                .GroupBy(v => v.PostId)
                .Select(g => new { PostId = g.Key, Score = g.Sum(x => x.Value) });

            var joined = from p in query
                         join s in scores on p.Id equals s.PostId into ps
                         from s in ps.DefaultIfEmpty()
                         select new { p, Score = (int?)s.Score ?? 0 };

            // sort
            switch ((rq.Sort ?? "created_desc").ToLowerInvariant())
            {
                case "created_asc":
                    joined = joined.OrderBy(x => x.p.CreatedAt);
                    break;
                case "score_desc":
                    joined = joined.OrderByDescending(x => x.Score).ThenByDescending(x => x.p.CreatedAt);
                    break;
                default: // created_desc
                    joined = joined.OrderByDescending(x => x.p.CreatedAt);
                    break;
            }

            var total = await query.CountAsync(ct);
            var items = await joined
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new PostDto
                {
                    Id = x.p.Id,
                    Title = x.p.Title,
                    AuthorDisplayName = x.p.Author!.DisplayName,
                    CreatedAt = x.p.CreatedAt,
                    Score = x.Score,
                    Tags = x.p.PostTags.Select(pt => pt.Tag!.Name).ToList(),
                    PreviewBody = x.p.Body.Length > 160 ? x.p.Body.Substring(0, 160) + "..." : x.p.Body,
                    CategoryId = x.p.CategoryId,
                    CategorySlug = x.p.Category != null ? x.p.Category.Slug : null,
                    CategoryName = x.p.Category != null ? x.p.Category.Name : null
                })
                .ToListAsync(ct);

            return new Paged<PostDto> { Page = page, PageSize = size, Total = total, Items = items };
        }
    }
}
