using Application.Comments.DTOs;
using Application.Comments.Queries.GetCommentsByPost;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Comments.Handlers
{
    public class GetCommentsByPostQueryHandler : IRequestHandler<GetCommentsByPostQuery, Paged<CommentDto>>
    {
        private readonly Application.Common.Interfaces.IApplicationDbContext _db;
        public GetCommentsByPostQueryHandler(Application.Common.Interfaces.IApplicationDbContext db) => _db = db;

        public async Task<Paged<CommentDto>> Handle(GetCommentsByPostQuery rq, CancellationToken ct)
        {
            var page = rq.Page <= 0 ? 1 : rq.Page;
            var size = rq.PageSize <= 0 ? 20 : rq.PageSize;

            var baseQuery = _db.Comments
                .Include(c => c.Author)
                .Where(c => c.PostId == rq.PostId);

            var total = await baseQuery.CountAsync(ct);

            var items = await baseQuery
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    Body = c.Body,
                    AuthorDisplayName = c.Author!.DisplayName,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(ct);

            return new Paged<CommentDto> { Page = page, PageSize = size, Total = total, Items = items };
        }
    }
}
