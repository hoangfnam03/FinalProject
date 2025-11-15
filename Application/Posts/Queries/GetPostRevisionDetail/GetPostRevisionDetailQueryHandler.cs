using Application.Common.Interfaces;
using Application.Posts.DTOs;
using Application.Posts.Queries.GetPostRevisionDetail;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Posts.Queries.GetPostRevisionDetail
{
    public class GetPostRevisionDetailQueryHandler : IRequestHandler<GetPostRevisionDetailQuery, RevisionDetailDto>
    {
        private readonly IApplicationDbContext _db;

        public GetPostRevisionDetailQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<RevisionDetailDto> Handle(GetPostRevisionDetailQuery request, CancellationToken ct)
        {
            var r = await _db.PostRevisions.AsNoTracking()
                        .Include(x => x.Editor)
                        .FirstOrDefaultAsync(x => x.PostId == request.PostId && x.Id == request.RevisionId, ct);

            if (r == null) throw new KeyNotFoundException("Revision not found.");

            return new RevisionDetailDto
            {
                Id = r.Id,
                PostId = r.PostId,
                EditorDisplayName = r.Editor != null ? r.Editor.DisplayName : "unknown",
                CreatedAt = r.CreatedAt,
                Summary = r.Summary,
                BeforeTitle = r.BeforeTitle,
                AfterTitle = r.AfterTitle,
                BeforeBody = r.BeforeBody,
                AfterBody = r.AfterBody
            };
        }
    }
}
