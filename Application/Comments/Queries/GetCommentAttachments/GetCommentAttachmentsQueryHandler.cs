using Application.Comments.DTOs;
using Application.Common.Interfaces;
using Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Comments.Queries.GetCommentAttachments
{
    public class GetCommentAttachmentsQueryHandler : IRequestHandler<GetCommentAttachmentsQuery, List<CommentAttachmentDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetCommentAttachmentsQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<List<CommentAttachmentDto>> Handle(GetCommentAttachmentsQuery request, CancellationToken ct)
        {
            var list = await _db.CommentAttachments.AsNoTracking()
                .Where(a => a.CommentId == request.CommentId)
                .OrderBy(a => a.CreatedAt)
                .Select(a => new CommentAttachmentDto
                {
                    Id = a.Id,
                    Type = a.Type == AttachmentType.Image ? "image" : "link",
                    CreatedAt = a.CreatedAt,
                    Url = a.FileUrl,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    Size = a.Size,
                    Caption = a.Caption,
                    LinkUrl = a.LinkUrl,
                    DisplayText = a.DisplayText
                })
                .ToListAsync(ct);

            return list;
        }
    }
}
