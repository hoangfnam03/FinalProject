using Application.Common.Interfaces;
using Application.Posts.Commands.UploadPostImage;
using Application.Posts.DTOs;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Posts.Commands.UploadPostImage
{
    public class UploadPostImageCommandHandler : IRequestHandler<UploadPostImageCommand, AttachmentDto>
    {
        private static readonly string[] Allowed = new[] { "image/png", "image/jpeg", "image/webp" };
        private const long MaxSize = 10 * 1024 * 1024;

        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly ITenantProvider _tenant;
        private readonly IFileStorage _storage;

        public UploadPostImageCommandHandler(
            IApplicationDbContext db, ICurrentUserService current, ITenantProvider tenant, IFileStorage storage)
        { _db = db; _current = current; _tenant = tenant; _storage = storage; }

        public async Task<AttachmentDto> Handle(UploadPostImageCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            var post = await _db.Posts.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == request.PostId, ct)
                        ?? throw new KeyNotFoundException("Post not found.");

            var file = request.File ?? throw new ArgumentException("File is required.");
            if (!Allowed.Contains(file.ContentType)) throw new InvalidOperationException("Unsupported content type.");
            if (file.Length <= 0 || file.Length > MaxSize) throw new InvalidOperationException("Invalid file size.");

            var tenantId = _tenant.GetTenantId();
            var url = await _storage.SaveAsync(file.OpenReadStream(), file.FileName, file.ContentType, $"uploads/posts/{post.Id}", ct);

            var att = new PostAttachment
            {
                PostId = post.Id,
                Type = AttachmentType.Image,
                FileUrl = url,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                Caption = request.Caption,
                CreatedById = me,
                TenantId = tenantId
            };
            _db.PostAttachments.Add(att);
            await _db.SaveChangesAsync(ct);

            return new AttachmentDto
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
