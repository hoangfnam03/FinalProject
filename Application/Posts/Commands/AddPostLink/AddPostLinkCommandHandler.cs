using Application.Common.Interfaces;
using Application.Posts.Commands.AddPostLink;
using Application.Posts.DTOs;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Posts.Commands.AddPostLink
{
    public class AddPostLinkCommandHandler : IRequestHandler<AddPostLinkCommand, AttachmentDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly ITenantProvider _tenant;

        public AddPostLinkCommandHandler(IApplicationDbContext db, ICurrentUserService current, ITenantProvider tenant)
        { _db = db; _current = current; _tenant = tenant; }

        public async Task<AttachmentDto> Handle(AddPostLinkCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            var post = await _db.Posts.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == request.PostId, ct)
                        ?? throw new KeyNotFoundException("Post not found.");

            var dto = request.Request;
            if (string.IsNullOrWhiteSpace(dto.LinkUrl)) throw new ArgumentException("LinkUrl is required.");

            var att = new PostAttachment
            {
                PostId = post.Id,
                Type = AttachmentType.Link,
                LinkUrl = dto.LinkUrl.Trim(),
                DisplayText = string.IsNullOrWhiteSpace(dto.DisplayText) ? dto.LinkUrl.Trim() : dto.DisplayText.Trim(),
                CreatedById = me,
                TenantId = _tenant.GetTenantId()
            };
            _db.PostAttachments.Add(att);
            await _db.SaveChangesAsync(ct);

            return new AttachmentDto
            {
                Id = att.Id,
                Type = "link",
                CreatedAt = att.CreatedAt,
                LinkUrl = att.LinkUrl,
                DisplayText = att.DisplayText
            };
        }
    }
}
