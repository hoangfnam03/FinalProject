using Application.Common.Interfaces;
using Application.Posts.Commands.RemoveAttachment;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Posts.Commands.RemoveAttachment
{
    public class RemoveAttachmentCommandHandler : IRequestHandler<RemoveAttachmentCommand, bool>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly IFileStorage _storage;

        public RemoveAttachmentCommandHandler(IApplicationDbContext db, ICurrentUserService current, IFileStorage storage)
        { _db = db; _current = current; _storage = storage; }

        public async Task<bool> Handle(RemoveAttachmentCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");

            var att = await _db.PostAttachments
                .Include(a => a.Post)
                .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.PostId == request.PostId, ct)
                ?? throw new KeyNotFoundException("Attachment not found.");

            // Quyền: chủ bài viết hoặc moderator
            var postAuthorId = att.Post.AuthorId;
            var meMember = await _db.Members.FindAsync(new object[] { me }, ct);
            var isOwner = postAuthorId == me;
            var isMod = (meMember?.IsModerator == true) || (meMember?.IsAdministrator == true);
            if (!isOwner && !isMod) throw new UnauthorizedAccessException("Forbidden.");

            // Nếu là file ảnh, xoá file vật lý (best-effort)
            if (!string.IsNullOrEmpty(att.FileUrl))
            {
                await _storage.DeleteAsync(att.FileUrl, ct);
            }

            _db.PostAttachments.Remove(att);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
