using Application.Common.Interfaces;
using Application.Posts.Commands.VotePost;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Posts.Handlers
{
    public class VotePostCommandHandler : IRequestHandler<VotePostCommand, (int score, int? myVote)>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public VotePostCommandHandler(IApplicationDbContext db, ICurrentUserService current)
        { _db = db; _current = current; }

        public async Task<(int score, int? myVote)> Handle(VotePostCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            if (request.Value != 1 && request.Value != -1) throw new ArgumentException("value must be 1 or -1");

            // Lấy post (cần AuthorId/Title cho notify & validate tồn tại)
            var post = await _db.Posts
                .AsNoTracking()
                .Where(p => p.Id == request.PostId)
                .Select(p => new { p.Id, p.AuthorId, p.Title })
                .FirstOrDefaultAsync(ct);
            if (post == null) throw new KeyNotFoundException("Post not found.");

            var vote = await _db.PostVotes
                .FirstOrDefaultAsync(v => v.PostId == request.PostId && v.MemberId == me, ct);

            if (vote == null)
            {
                _db.PostVotes.Add(new Domain.Entities.PostVote
                {
                    PostId = request.PostId,
                    MemberId = me,
                    Value = request.Value
                });
            }
            else
            {
                if (vote.Value == request.Value)
                    _db.PostVotes.Remove(vote); // toggle off
                else
                    vote.Value = request.Value;
            }

            await _db.SaveChangesAsync(ct);

            // Notify khi upvote (request.Value == 1) và không tự upvote bài mình
            if (request.Value == 1 && post.AuthorId != me)
            {
                _db.Notifications.Add(new Notification
                {
                    RecipientId = post.AuthorId,
                    ActorId = me,
                    Type = NotificationType.PostVoted,
                    PostId = post.Id,
                    DataJson = post.Title
                });
                await _db.SaveChangesAsync(ct);
            }

            var score = await _db.PostVotes.Where(v => v.PostId == request.PostId).SumAsync(v => v.Value, ct);
            var my = await _db.PostVotes.Where(v => v.PostId == request.PostId && v.MemberId == me)
                                        .Select(v => (int?)v.Value).FirstOrDefaultAsync(ct);

            return (score, my);
        }
    }
}
