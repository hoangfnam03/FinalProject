using Application.Comments.Commands.UploadCommentImage;
using Application.Comments.DTOs;
using Application.Common.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Comments.Commands.UploadCommentImage
{
    public class UploadCommentImageCommandHandler : IRequestHandler<UploadCommentImageCommand, CommentAttachmentDto>
    {
        private static readonly string[] Allowed = new[] { "image/png", "image/jpeg", "image/webp" };
        private const long MaxSize = 10 * 1024 * 1024;

        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly ITenantProvider _tenant;
        private readonly IFileStorage _storage;

        public UploadCommentImageCommandHandler(
            IApplicationDbContext db, ICurrentUserService current, ITenantProvider tenant, IFileStorage storage)
        { _db = db; _current = current; _tenant = tenant; _storage = storage; }

        public async Task<CommentAttachmentDto> Handle(UploadCommentImageCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            var cmt = await _db.Comments.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == request.CommentId, ct)
                        ?? throw new KeyNotFoundException("Comment not found.");

            // Quyền: chủ comment hoặc moderator
            var meMember = await _db.Members.FindAsync(new object[] { me }, ct);
            var isOwner = cmt.AuthorId == me;
            var isMod = (meMember?.IsModerator == true) || (meMember?.IsAdministrator == true);
            if (!isOwner && !isMod) throw new UnauthorizedAccessException("Forbidden.");

            var file = request.File ?? throw new ArgumentException("File is required.");
            if (!Allowed.Contains(file.ContentType)) throw new InvalidOperationException("Unsupported content type.");
            if (file.Length <= 0 || file.Length > MaxSize) throw new InvalidOperationException("Invalid file size.");

            var url = await _storage.SaveAsync(file.OpenReadStream(), file.FileName, file.ContentType, $"uploads/comments/{cmt.Id}", ct);

            var att = new CommentAttachment
            {
                CommentId = cmt.Id,
                Type = AttachmentType.Image,
                FileUrl = url,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                Caption = request.Caption,
                CreatedById = me,
                TenantId = _tenant.GetTenantId()
            };

            _db.CommentAttachments.Add(att);
            await _db.SaveChangesAsync(ct);

            return new CommentAttachmentDto
            {
                Id = att.Id,
                Type = "image",
                CreatedAt = att.CreatedAt,
                Url = att.FileUrl,
                FileName = att.FileName,
                ContentType = att.ContentType,
                Size = att.Size,
                Caption = att.Caption
            };
        }
    }
}
