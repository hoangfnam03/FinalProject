using Application.Common.Interfaces;
using Application.Posts.DTOs;
using Domain.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Posts.Queries.GetPostAttachments
{
    public class GetPostAttachmentsQueryHandler : IRequestHandler<GetPostAttachmentsQuery, List<AttachmentDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetPostAttachmentsQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<List<AttachmentDto>> Handle(GetPostAttachmentsQuery request, CancellationToken ct)
        {
            var list = await _db.PostAttachments.AsNoTracking()
                .Where(a => a.PostId == request.PostId)
                .OrderBy(a => a.CreatedAt)
                .Select(a => new AttachmentDto
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
