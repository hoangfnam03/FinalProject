using Application.Comments.Commands.RemoveCommentAttachment;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Comments.Commands.RemoveCommentAttachment
{
    public class RemoveCommentAttachmentCommandHandler : IRequestHandler<RemoveCommentAttachmentCommand, bool>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly IFileStorage _storage;

        public RemoveCommentAttachmentCommandHandler(IApplicationDbContext db, ICurrentUserService current, IFileStorage storage)
        { _db = db; _current = current; _storage = storage; }

        public async Task<bool> Handle(RemoveCommentAttachmentCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");

            var att = await _db.CommentAttachments
                .Include(a => a.Comment)
                .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.CommentId == request.CommentId, ct)
                ?? throw new KeyNotFoundException("Attachment not found.");

            // Quyền: chủ comment hoặc moderator
            var meMember = await _db.Members.FindAsync(new object[] { me }, ct);
            var isOwner = att.Comment.AuthorId == me;
            var isMod = (meMember?.IsModerator == true) || (meMember?.IsAdministrator == true);
            if (!isOwner && !isMod) throw new UnauthorizedAccessException("Forbidden.");

            // Nếu có file vật lý thì xoá
            if (!string.IsNullOrEmpty(att.FileUrl))
                await _storage.DeleteAsync(att.FileUrl, ct);

            _db.CommentAttachments.Remove(att);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
