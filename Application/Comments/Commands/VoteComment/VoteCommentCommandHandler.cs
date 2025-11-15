using Application.Comments.Commands.VoteComment;
using Application.Comments.DTOs;
using Application.Common.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Comments.Handlers
{
    public class VoteCommentCommandHandler : IRequestHandler<VoteCommentCommand, VoteCommentResponse>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public VoteCommentCommandHandler(IApplicationDbContext db, ICurrentUserService current)
        { _db = db; _current = current; }

        public async Task<VoteCommentResponse> Handle(VoteCommentCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            if (request.Value != 1 && request.Value != -1)
                throw new ArgumentException("value must be 1 or -1");

            // Lấy comment (cần AuthorId/PostId/Body cho notify và validate tồn tại)
            var comment = await _db.Comments
                .AsNoTracking()
                .Where(c => c.Id == request.CommentId)
                .Select(c => new { c.Id, c.AuthorId, c.PostId, c.Body })
                .FirstOrDefaultAsync(ct);
            if (comment == null) throw new KeyNotFoundException("Comment not found.");

            var vote = await _db.CommentVotes
                .FirstOrDefaultAsync(v => v.CommentId == request.CommentId && v.MemberId == me, ct);

            if (vote == null)
            {
                _db.CommentVotes.Add(new Domain.Entities.CommentVote
                {
                    CommentId = request.CommentId,
                    MemberId = me,
                    Value = request.Value
                });
            }
            else
            {
                if (vote.Value == request.Value)
                    _db.CommentVotes.Remove(vote); // toggle off
                else
                    vote.Value = request.Value;
            }

            await _db.SaveChangesAsync(ct);

            // Notify khi upvote (request.Value == 1) và không tự vote comment của mình
            if (request.Value == 1 && comment.AuthorId != me)
            {
                var excerpt = (comment.Body ?? string.Empty);
                if (excerpt.Length > 140) excerpt = excerpt.Substring(0, 140) + "...";

                _db.Notifications.Add(new Notification
                {
                    RecipientId = comment.AuthorId,
                    ActorId = me,
                    Type = NotificationType.CommentVoted,
                    PostId = comment.PostId,
                    CommentId = comment.Id,
                    DataJson = excerpt
                });
                await _db.SaveChangesAsync(ct);
            }

            var score = await _db.CommentVotes
                .Where(v => v.CommentId == request.CommentId)
                .SumAsync(v => v.Value, ct);

            var myVote = await _db.CommentVotes
                .Where(v => v.CommentId == request.CommentId && v.MemberId == me)
                .Select(v => (int?)v.Value)
                .FirstOrDefaultAsync(ct);

            return new VoteCommentResponse { Score = score, MyVote = myVote };
        }
    }
}
