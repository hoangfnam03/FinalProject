using Application.Comments.Commands.AddCommentLink;
using Application.Comments.DTOs;
using Application.Common.Interfaces;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Comments.Commands.AddCommentLink
{
    public class AddCommentLinkCommandHandler : IRequestHandler<AddCommentLinkCommand, CommentAttachmentDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly ITenantProvider _tenant;

        public AddCommentLinkCommandHandler(IApplicationDbContext db, ICurrentUserService current, ITenantProvider tenant)
        { _db = db; _current = current; _tenant = tenant; }

        public async Task<CommentAttachmentDto> Handle(AddCommentLinkCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            var cmt = await _db.Comments.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Id == request.CommentId, ct)
                        ?? throw new KeyNotFoundException("Comment not found.");

            var meMember = await _db.Members.FindAsync(new object[] { me }, ct);
            var isOwner = cmt.AuthorId == me;
            var isMod = (meMember?.IsModerator == true) || (meMember?.IsAdministrator == true);
            if (!isOwner && !isMod) throw new UnauthorizedAccessException("Forbidden.");

            var dto = request.Request;
            if (string.IsNullOrWhiteSpace(dto.LinkUrl)) throw new ArgumentException("LinkUrl is required.");

            var att = new CommentAttachment
            {
                CommentId = cmt.Id,
                Type = AttachmentType.Link,
                LinkUrl = dto.LinkUrl.Trim(),
                DisplayText = string.IsNullOrWhiteSpace(dto.DisplayText) ? dto.LinkUrl.Trim() : dto.DisplayText.Trim(),
                CreatedById = me,
                TenantId = _tenant.GetTenantId()
            };

            _db.CommentAttachments.Add(att);
            await _db.SaveChangesAsync(ct);

            return new CommentAttachmentDto
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
