using Application.Comments.Commands.UpdateComment;
using Application.Comments.DTOs;
using Application.Common.Interfaces;
using Application.Posts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using System.Linq;

namespace Application.Comments.Commands.UpdateComment
{
    public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, CommentDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public UpdateCommentCommandHandler(IApplicationDbContext db, ICurrentUserService current)
        { _db = db; _current = current; }

        public async Task<CommentDto> Handle(UpdateCommentCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");

            var cmt = await _db.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == request.CommentId, ct)
                ?? throw new KeyNotFoundException("Comment not found.");

            var myMember = await _db.Members.FindAsync(new object[] { me }, ct);
            var isOwner = cmt.AuthorId == me;
            var isMod = (myMember?.IsModerator == true) || (myMember?.IsAdministrator == true);
            if (!isOwner && !isMod) throw new UnauthorizedAccessException("Forbidden.");

            var dto = request.Request;
            var beforeBody = cmt.Body;

            if (!string.IsNullOrWhiteSpace(dto.Body))
                cmt.Body = dto.Body;

            // Lưu revision
            _db.CommentRevisions.Add(new CommentRevision
            {
                CommentId = cmt.Id,
                BeforeBody = beforeBody,
                AfterBody = cmt.Body,
                EditorId = me,
                Summary = dto.Summary ?? "Cập nhật bình luận"
            });

            await _db.SaveChangesAsync(ct);

            // tính score
            var score = await _db.CommentVotes.Where(v => v.CommentId == cmt.Id).SumAsync(v => v.Value, ct);

            return new CommentDto
            {
                Id = cmt.Id,
                PostId = cmt.PostId,
                Body = cmt.Body,
                AuthorDisplayName = cmt.Author?.DisplayName ?? "unknown",
                CreatedAt = cmt.CreatedAt
                // Nếu bạn có thêm Score/MyVote vào CommentDto thì set ở đây
            };
        }
    }
}
