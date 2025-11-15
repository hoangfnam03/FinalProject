using Application.Comments.Commands.AddComment;
using Application.Comments.DTOs;
using Application.Common.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Comments.Handlers
{
    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, CommentDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public AddCommentCommandHandler(IApplicationDbContext db, ICurrentUserService current)
        {
            _db = db; _current = current;
        }

        public async Task<CommentDto> Handle(AddCommentCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            var body = (request.Request.Body ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(body))
                throw new ArgumentException("Comment body is required.");

            // Lấy thông tin Post (cần AuthorId để notify)
            var post = await _db.Posts
                .AsNoTracking()
                .Where(p => p.Id == request.PostId)
                .Select(p => new { p.Id, p.AuthorId, p.Title })
                .FirstOrDefaultAsync(ct);
            if (post == null) throw new KeyNotFoundException("Post not found.");

            // Nếu là reply, lấy parent comment (cần AuthorId để notify)
            long? parentId = request.Request.ParentCommentId;
            var parentComment = parentId.HasValue
                ? await _db.Comments.AsNoTracking()
                    .Where(c => c.Id == parentId.Value)
                    .Select(c => new { c.Id, c.AuthorId, c.PostId })
                    .FirstOrDefaultAsync(ct)
                : null;

            var cmt = new Comment
            {
                PostId = request.PostId,
                AuthorId = me,
                Body = body,
                ParentCommentId = parentId
            };
            _db.Comments.Add(cmt);
            await _db.SaveChangesAsync(ct); // cần để có cmt.Id

            // Tạo notification (đơn luồng, tuần tự)
            var actorId = me;
            var excerpt = body.Length > 140 ? body.Substring(0, 140) + "..." : body;

            if (parentComment == null)
            {
                var recipientId = post.AuthorId;
                if (recipientId != actorId)
                {
                    _db.Notifications.Add(new Notification
                    {
                        RecipientId = recipientId,
                        ActorId = actorId,
                        Type = NotificationType.PostCommented,
                        PostId = post.Id,
                        CommentId = cmt.Id,
                        DataJson = excerpt
                    });
                    await _db.SaveChangesAsync(ct);
                }
            }
            else
            {
                var recipientId = parentComment.AuthorId;
                if (recipientId != actorId)
                {
                    _db.Notifications.Add(new Notification
                    {
                        RecipientId = recipientId,
                        ActorId = actorId,
                        Type = NotificationType.CommentReplied,
                        PostId = post.Id,
                        CommentId = cmt.Id,
                        DataJson = excerpt
                    });
                    await _db.SaveChangesAsync(ct);
                }
            }

            // lấy display name
            var authorName = await _db.Members
                .AsNoTracking()
                .Where(m => m.Id == me)
                .Select(m => m.DisplayName)
                .FirstOrDefaultAsync(ct) ?? "unknown";

            return new CommentDto
            {
                Id = cmt.Id,
                PostId = cmt.PostId,
                Body = cmt.Body,
                AuthorDisplayName = authorName,
                CreatedAt = cmt.CreatedAt
            };
        }
    }
}
