using Application.Common.Interfaces;
using Application.Comments.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Comments.Queries.GetCommentRevisionDetail
{
    public class GetCommentRevisionDetailQueryHandler : IRequestHandler<GetCommentRevisionDetailQuery, CommentRevisionDetailDto>
    {
        private readonly IApplicationDbContext _db;
        public GetCommentRevisionDetailQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<CommentRevisionDetailDto> Handle(GetCommentRevisionDetailQuery request, CancellationToken ct)
        {
            var r = await _db.CommentRevisions.AsNoTracking()
                        .Include(x => x.Editor)
                        .FirstOrDefaultAsync(x => x.CommentId == request.CommentId && x.Id == request.RevisionId, ct);

            if (r == null) throw new KeyNotFoundException("Revision not found.");

            return new CommentRevisionDetailDto
            {
                Id = r.Id,
                CommentId = r.CommentId,
                EditorDisplayName = r.Editor != null ? r.Editor.DisplayName : "unknown",
                CreatedAt = r.CreatedAt,
                Summary = r.Summary,
                BeforeBody = r.BeforeBody,
                AfterBody = r.AfterBody
            };
        }
    }
}
