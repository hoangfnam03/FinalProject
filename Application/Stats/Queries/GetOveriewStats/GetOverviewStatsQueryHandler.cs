using Application.Common.Interfaces;
using Application.Stats.DTOs;
using Application.Stats.Queries;
using Application.Stats.Queries.GetOveriewStats;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stats.Queries.GetOveriewStats
{
    public class GetOverviewStatsQueryHandler : IRequestHandler<GetOverviewStatsQuery, OverviewStatsDto>
    {
        private readonly IApplicationDbContext _db;

        public GetOverviewStatsQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<OverviewStatsDto> Handle(GetOverviewStatsQuery request, CancellationToken ct)
        {
            var totalQuestions = await _db.Posts.LongCountAsync(ct);
            var totalComments = await _db.Comments.LongCountAsync(ct);
            var totalUsers = await _db.Members.LongCountAsync(ct);

            var postVotesCount = await _db.PostVotes.LongCountAsync(ct);
            var commentVotesCount = await _db.CommentVotes.LongCountAsync(ct);
            var totalVotes = postVotesCount + commentVotesCount;

            var top = request.Top <= 0 ? 10 : request.Top;

            var topTags = await (
                from pt in _db.PostTags
                group pt by pt.TagId into g
                orderby g.Count() descending
                select new { TagId = g.Key, Count = g.Count() }
            )
            .Join(_db.Tags, g => g.TagId, t => t.Id,
                (g, t) => new TopTagDto { Id = t.Id, Name = t.Name, Slug = t.Slug, Count = g.Count })
            .Take(top)
            .AsNoTracking()
            .ToListAsync(ct);

            return new OverviewStatsDto
            {
                TotalQuestions = totalQuestions,
                TotalComments = totalComments,
                TotalUsers = totalUsers,
                TotalVotes = totalVotes,
                TopTags = topTags
            };
        }

    }
}

