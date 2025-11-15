using Application.Common.Interfaces;
using Application.Common.Utils;
using Application.Posts.Commands.CreatePost;
using Application.Posts.DTOs;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Posts.Handlers
{
    public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, PostDetailDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly ITenantProvider _tenant;

        public CreatePostCommandHandler(IApplicationDbContext db, ICurrentUserService current, ITenantProvider tenant)
        {
            _db = db; _current = current; _tenant = tenant;
        }

        public async Task<PostDetailDto> Handle(CreatePostCommand request, CancellationToken ct)
        {
            var dto = request.Request;
            var authorId = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");

            Category? category = null;

            if (!string.IsNullOrWhiteSpace(dto.CategorySlug))
            {
                category = await _db.Categories.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Slug == dto.CategorySlug, ct)
                    ?? throw new ArgumentException("CategorySlug không tồn tại.");

                if (category.IsHidden)
                    throw new UnauthorizedAccessException("Không thể đăng vào category ẩn.");
            }
            else if (dto.CategoryId.HasValue)
            {
                category = await _db.Categories.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == dto.CategoryId.Value, ct)
                    ?? throw new ArgumentException("CategoryId không tồn tại.");

                if (category.IsHidden)
                    throw new UnauthorizedAccessException("Không thể đăng vào category ẩn.");
            }


            var post = new Post
            {
                Title = dto.Title.Trim(),
                Body = dto.Body,
                AuthorId = authorId,
                CategoryId = category?.Id
            };
            _db.Posts.Add(post);
            await _db.SaveChangesAsync(ct);

            // Tags
            var tags = new List<Tag>();
            if (dto.Tags != null && dto.Tags.Count > 0)
            {
                var tenantId = _tenant.GetTenantId();
                var slugs = dto.Tags.Select(SlugGenerator.Slugify)
                                    .Where(s => !string.IsNullOrWhiteSpace(s))
                                    .Distinct()
                                    .ToList();

                var existing = await _db.Tags.Where(t => slugs.Contains(t.Slug)).ToListAsync(ct);
                var missing = slugs.Except(existing.Select(e => e.Slug)).ToList();

                foreach (var ms in missing)
                {
                    var name = dto.Tags.First(t => SlugGenerator.Slugify(t) == ms);
                    var tag = new Tag { Name = name, Slug = ms, TenantId = tenantId };
                    _db.Tags.Add(tag);
                    existing.Add(tag);
                }
                await _db.SaveChangesAsync(ct);

                tags = existing;
                foreach (var tag in tags)
                    _db.PostTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });

                await _db.SaveChangesAsync(ct);
            }
            var catName = category?.Name;
            var catSlug = category?.Slug;
            var catId = category?.Id;

            return new PostDetailDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                CreatedAt = post.CreatedAt,
                AuthorDisplayName = (await _db.Members.FindAsync(new object[] { authorId }, ct))?.DisplayName ?? "unknown",
                Score = 0,
                MyVote = null,
                Tags = tags.Select(t => t.Name).ToList(),
                CategoryId = catId,
                CategorySlug = catSlug,
                CategoryName = catName
            };
        }
    }
}
