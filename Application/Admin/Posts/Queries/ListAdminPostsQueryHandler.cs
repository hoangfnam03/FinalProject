using Application.Admin.Documents.DTOs;
using Application.Admin.Questions.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Questions.Queries
{
    public class ListAdminPostsQueryHandler : IRequestHandler<ListAdminPostsQuery, Paged<AdminPostListItemDto>>
    {
        private readonly IApplicationDbContext _db;

        public ListAdminPostsQueryHandler(IApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Paged<AdminPostListItemDto>> Handle(ListAdminPostsQuery request, CancellationToken ct)
        {
            // Validate / normalize paging
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 200);

            var keyword = request.Keyword?.Trim();
            var status = request.Status?.Trim();

            // Build query:
            // - join posts -> members by Member.UserId (Post.AuthorId is Guid)
            // - join posts -> categories (inner join, same as original)
            var query =
                from p in _db.Posts.AsNoTracking()
                join m in _db.Members.AsNoTracking() on p.AuthorId equals m.UserId
                join c in _db.Categories.AsNoTracking() on p.CategoryId equals c.Id
                select new
                {
                    Post = p,
                    AuthorName = m.DisplayName,
                    CategoryName = c.Name,
                    CategoryId = c.Id,
                    CommentCount = _db.Comments.Count(x => x.PostId == p.Id)
                };

            // Exclude soft-deleted posts (SoftDeletableEntity => IsDeleted)
            query = query.Where(x => !x.Post.IsDeleted);

            if (!string.IsNullOrWhiteSpace(keyword))    
            {
                query = query.Where(x => x.Post.Title.Contains(keyword!));
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == request.CategoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                if (status.Equals("Answered", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.CommentCount > 0);
                else if (status.Equals("Unanswered", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(x => x.CommentCount == 0);
            }

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.Post.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AdminPostListItemDto
                {
                    Id = x.Post.Id,
                    Title = x.Post.Title,
                    AuthorName = x.AuthorName,
                    CategoryName = x.CategoryName,
                    CategoryId = x.CategoryId,
                    Comments = x.CommentCount,
                    Status = x.CommentCount > 0 ? "Answered" : "Unanswered",
                    CreatedAt = x.Post.CreatedAt
                })
                .ToListAsync(ct);

            return new Paged<AdminPostListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            };
        }
    }
}
