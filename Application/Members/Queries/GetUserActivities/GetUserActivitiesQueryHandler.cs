// Application/Members/Queries/GetUserActivities/GetUserActivitiesQueryHandler.cs
using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Members.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.Queries.GetUserActivities
{
    public class GetUserActivitiesQueryHandler : IRequestHandler<GetUserActivitiesQuery, Paged<ActivityDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetUserActivitiesQueryHandler(IApplicationDbContext db) { _db = db; }

        private static string Excerpt(string? s, int max = 160)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            if (s.Length <= max) return s;
            return s.Substring(0, max);
        }

        public async Task<Paged<ActivityDto>> Handle(GetUserActivitiesQuery request, CancellationToken ct)
        {
            var uid = request.UserId;

            // 1) Lấy dữ liệu tối thiểu từ DB (materialize trước)
            var postsRaw = await _db.Posts.AsNoTracking()
                .Where(p => p.AuthorId == uid)
                .Select(p => new { p.Id, p.Title, p.Body, p.CreatedAt })
                .ToListAsync(ct);

            var commentsRaw = await _db.Comments.AsNoTracking()
                .Where(c => c.AuthorId == uid)
                .Select(c => new { c.Id, c.PostId, c.Body, c.CreatedAt })
                .ToListAsync(ct);

            var postVotesRaw = await _db.PostVotes.AsNoTracking()
                .Where(v => v.MemberId == uid)
                .Select(v => new { v.Id, v.PostId, v.Value, v.CreatedAt })
                .ToListAsync(ct);

            var commentVotesRaw = await _db.CommentVotes.AsNoTracking()
                .Where(v => v.MemberId == uid)
                .Select(v => new { v.Id, v.CommentId, v.Value, v.CreatedAt })
                .ToListAsync(ct);

            // 2) Map sang ActivityDto in-memory (an toàn null/substring)
            var posts = postsRaw.Select(p => new ActivityDto
            {
                Id = p.Id,
                Type = "post",
                CreatedAt = p.CreatedAt,
                PostId = p.Id,
                PostTitle = p.Title,
                Excerpt = Excerpt(p.Body)
            });

            var comments = commentsRaw.Select(c => new ActivityDto
            {
                Id = c.Id,
                Type = "comment",
                CreatedAt = c.CreatedAt,
                PostId = c.PostId,
                CommentId = c.Id,
                Excerpt = Excerpt(c.Body)
            });

            var postVotes = postVotesRaw.Select(v => new ActivityDto
            {
                Id = v.Id,
                Type = "vote_post",
                CreatedAt = v.CreatedAt,
                PostId = v.PostId,
                DeltaScore = v.Value
            });

            var commentVotes = commentVotesRaw.Select(v => new ActivityDto
            {
                Id = v.Id,
                Type = "vote_comment",
                CreatedAt = v.CreatedAt,
                CommentId = v.CommentId,
                DeltaScore = v.Value
            });

            // 3) Gộp + sort + phân trang in-memory
            var union = posts.Concat(comments).Concat(postVotes).Concat(commentVotes);
            var total = union.Count();

            var items = union
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new Paged<ActivityDto>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total,
                Items = items
            };
        }
    }
}
