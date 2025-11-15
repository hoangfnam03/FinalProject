using Application.Common.Interfaces;
using Application.Common.Utils;
using Application.Posts.Commands.UpdatePost;
using Application.Posts.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain.Entities; // 👈 cần để dùng PostRevision

namespace Application.Posts.Handlers
{
    public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, PostDetailDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;
        private readonly ITenantProvider _tenant;

        public UpdatePostCommandHandler(IApplicationDbContext db, ICurrentUserService current, ITenantProvider tenant)
        { _db = db; _current = current; _tenant = tenant; }

        public async Task<PostDetailDto> Handle(UpdatePostCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");

            var post = await _db.Posts
                .Include(p => p.Author)
                .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
                ?? throw new KeyNotFoundException("Post not found.");

            // quyền: owner hoặc moderator
            var myMember = await _db.Members.FindAsync(new object[] { me }, ct);
            var isOwner = post.AuthorId == me;
            var isMod = (myMember?.IsModerator == true) || (myMember?.IsAdministrator == true);
            if (!isOwner && !isMod) throw new UnauthorizedAccessException("Forbidden.");

            var dto = request.Request;

            // 📌 Snapshot BEFORE để ghi vào revision
            var beforeTitle = post.Title;
            var beforeBody = post.Body;

            // cập nhật title/body
            if (!string.IsNullOrWhiteSpace(dto.Title)) post.Title = dto.Title.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Body)) post.Body = dto.Body;

            // cập nhật tags nếu gửi lên
            if (dto.Tags != null)
            {
                var slugs = dto.Tags.Select(SlugGenerator.Slugify).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                var tenantId = _tenant.GetTenantId();
                var existing = await _db.Tags.Where(t => slugs.Contains(t.Slug)).ToListAsync(ct);
                var missing = slugs.Except(existing.Select(e => e.Slug)).ToList();

                foreach (var ms in missing)
                {
                    var name = dto.Tags.First(t => SlugGenerator.Slugify(t) == ms);
                    var tag = new Domain.Entities.Tag { Name = name, Slug = ms, TenantId = tenantId };
                    _db.Tags.Add(tag);
                    existing.Add(tag);
                }
                await _db.SaveChangesAsync(ct);

                // replace set
                _db.PostTags.RemoveRange(post.PostTags);
                await _db.SaveChangesAsync(ct);

                foreach (var tag in existing)
                    _db.PostTags.Add(new Domain.Entities.PostTag { PostId = post.Id, TagId = tag.Id });
                await _db.SaveChangesAsync(ct);

                // reload navigation
                post = await _db.Posts
                    .Include(p => p.Author)
                    .Include(p => p.PostTags).ThenInclude(pt => pt.Tag)
                    .Include(p => p.Category)
                    .FirstAsync(p => p.Id == post.Id, ct);
            }

            // Xử lý category
            if (dto.RemoveCategory == true)
            {
                post.CategoryId = null;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(dto.CategorySlug))
                {
                    var cat = await _db.Categories.AsNoTracking()
                        .FirstOrDefaultAsync(c => c.Slug == dto.CategorySlug, ct)
                        ?? throw new ArgumentException("CategorySlug không tồn tại.");

                    if (cat.IsHidden && !isMod)
                        throw new UnauthorizedAccessException("Không thể chuyển vào category ẩn.");

                    post.CategoryId = cat.Id;
                }
                else if (dto.CategoryId.HasValue)
                {
                    if (dto.CategoryId.Value == 0)
                    {
                        post.CategoryId = null; // quy ước: 0 = bỏ category
                    }
                    else
                    {
                        var cat = await _db.Categories.AsNoTracking()
                            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId.Value, ct)
                            ?? throw new ArgumentException("CategoryId không tồn tại.");

                        if (cat.IsHidden && !isMod)
                            throw new UnauthorizedAccessException("Không thể chuyển vào category ẩn.");

                        post.CategoryId = cat.Id;
                    }
                }
            }

            // 📌 Ghi REVISION (sau khi đã gán title/body/category/tags mới)
            _db.PostRevisions.Add(new PostRevision
            {
                PostId = post.Id,
                BeforeTitle = beforeTitle,
                AfterTitle = post.Title,
                BeforeBody = beforeBody,
                AfterBody = post.Body,
                EditorId = me,
                Summary = "Cập nhật bài viết"
                // CreatedAt sẽ do AuditableInterceptor của bạn set (nếu có)
            });

            await _db.SaveChangesAsync(ct);

            var score = await _db.PostVotes.Where(v => v.PostId == post.Id).SumAsync(v => v.Value, ct);
            var myVote = await _db.PostVotes.Where(v => v.PostId == post.Id && v.MemberId == me).Select(v => (int?)v.Value).FirstOrDefaultAsync(ct);

            return new PostDetailDto
            {
                Id = post.Id,
                Title = post.Title,
                Body = post.Body,
                AuthorDisplayName = post.Author?.DisplayName ?? "unknown",
                CreatedAt = post.CreatedAt,
                Score = score,
                MyVote = myVote,
                Tags = post.PostTags.Select(pt => pt.Tag!.Name).ToList(),
                CategoryId = post.CategoryId,
                CategorySlug = post.Category?.Slug,
                CategoryName = post.Category?.Name
            };
        }
    }
}
