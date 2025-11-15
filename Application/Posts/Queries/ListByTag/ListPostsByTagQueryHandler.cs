using Application.Common.Models;
using Application.Posts.DTOs;
using Application.Posts.Queries.ListByTag;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Posts.Handlers
{
    public class ListPostsByTagQueryHandler : IRequestHandler<ListPostsByTagQuery, Paged<PostDto>>
    {
        private readonly Application.Common.Interfaces.IApplicationDbContext _db;
        public ListPostsByTagQueryHandler(Application.Common.Interfaces.IApplicationDbContext db) => _db = db;

        public async Task<Paged<PostDto>> Handle(ListPostsByTagQuery rq, CancellationToken ct)
        {
            var page = rq.Page <= 0 ? 1 : rq.Page;
            var size = rq.PageSize <= 0 ? 20 : rq.PageSize;

            var posts = _db.Posts
                .Include(p => p.Author)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .Where(p => p.PostTags.Any(pt => pt.Tag!.Slug == rq.TagSlug));

            var total = await posts.CountAsync(ct);

            var scores = _db.PostVotes
                .GroupBy(v => v.PostId)
                .Select(g => new { PostId = g.Key, Score = g.Sum(x => x.Value) });

            var joined = from p in posts
                         join s in scores on p.Id equals s.PostId into ps
                         from s in ps.DefaultIfEmpty()
                         select new { p, Score = (int?)s.Score ?? 0 };

            var items = await joined
                .OrderByDescending(x => x.p.CreatedAt)
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
