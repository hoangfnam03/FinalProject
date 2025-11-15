using Application.Common.Interfaces;
using Application.Stats.DTOs;
using Application.Stats.Queries;
using Application.Stats.Queries.GetMyStats;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stats.Queries.GetMyStats
{
    public class GetMyStatsQueryHandler : IRequestHandler<GetMyStatsQuery, UserStatsDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public GetMyStatsQueryHandler(IApplicationDbContext db, ICurrentUserService current)
        {
            _db = db; _current = current;
        }

        public async Task<UserStatsDto> Handle(GetMyStatsQuery request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            return await ComputeUserStats(_db, me, ct);
        }

        internal static async Task<UserStatsDto> ComputeUserStats(IApplicationDbContext db, long memberId, CancellationToken ct)
        {
            // counts cơ bản
            var questions = await db.Posts.CountAsync(p => p.AuthorId == memberId, ct);
            var comments = await db.Comments.CountAsync(c => c.AuthorId == memberId, ct);
            var postVotesByUser = await db.PostVotes.CountAsync(v => v.MemberId == memberId, ct);
            var commentVotesByUser = await db.CommentVotes.CountAsync(v => v.MemberId == memberId, ct);
            var votes = postVotesByUser + commentVotesByUser;

            // reputation: tổng vote nhận được trên post & comment của user
            var postIdsQuery = db.Posts.Where(p => p.AuthorId == memberId).Select(p => p.Id);
            var postRep = await db.PostVotes
                .Where(v => postIdsQuery.Contains(v.PostId))
                .SumAsync(v => (int?)v.Value, ct) ?? 0;

            var cmtIdsQuery = db.Comments.Where(c => c.AuthorId == memberId).Select(c => c.Id);
            var cmtRep = await db.CommentVotes
                .Where(v => cmtIdsQuery.Contains(v.CommentId))
                .SumAsync(v => (int?)v.Value, ct) ?? 0;

            var reputation = postRep + cmtRep;

            return new UserStatsDto
            {
                Questions = questions,
                Comments = comments,
                Votes = votes,
                Reputation = reputation
            };
        }

    }
}

