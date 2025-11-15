using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Posts.DTOs;
using Application.Search.Queries;
using Application.Search.Queries.SearchPosts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Search.Queries.SearchPosts
{
    public class SearchPostsQueryHandler : IRequestHandler<SearchPostsQuery, Paged<PostDto>>
    {
        private readonly IApplicationDbContext _db;
        public SearchPostsQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<Paged<PostDto>> Handle(SearchPostsQuery rq, CancellationToken ct)
        {
            var page = rq.Page <= 0 ? 1 : rq.Page;
            var size = rq.PageSize <= 0 ? 20 : rq.PageSize;

            var q = (rq.Q ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return new Paged<PostDto> { Page = page, PageSize = size, Total = 0, Items = new List<PostDto>() };

            // Base query
            var posts = _db.Posts
                .Include(p => p.Author)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .AsNoTracking()
                .Where(p =>
                    p.Title.Contains(q) ||
                    p.Body.Contains(q) ||
                    p.PostTags.Any(pt => pt.Tag!.Slug.Contains(q) || pt.Tag!.Name.Contains(q))
                );

            // Precompute scores from votes
            var voteScores =
                _db.PostVotes.GroupBy(v => v.PostId)
                .Select(g => new { PostId = g.Key, Score = g.Sum(x => x.Value) });

            // Relevance approximation (title > tags > body)
            var joined = from p in posts
                         join s in voteScores on p.Id equals s.PostId into ps
                         from s in ps.DefaultIfEmpty()
                         let titleMatch = p.Title.Contains(q) ? 1 : 0
                         let tagMatch = p.PostTags.Any(pt => pt.Tag!.Slug.Contains(q) || pt.Tag!.Name.Contains(q)) ? 1 : 0
                         let bodyMatch = p.Body.Contains(q) ? 1 : 0
                         select new
                         {
                             p,
                             Score = (int?)s.Score ?? 0,
                             Relevance = 3 * titleMatch + 2 * tagMatch + 1 * bodyMatch
                         };

            // Sort
            var sort = (rq.Sort ?? "relevance").ToLowerInvariant();
            joined = sort switch
            {
                "created_desc" => joined.OrderByDescending(x => x.p.CreatedAt),
                "score_desc" => joined.OrderByDescending(x => x.Score).ThenByDescending(x => x.p.CreatedAt),
                _ => joined.OrderByDescending(x => x.Relevance)
                                        .ThenByDescending(x => x.Score)
                                        .ThenByDescending(x => x.p.CreatedAt)
            };

            var total = await posts.CountAsync(ct);

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
