using Application.Categories.Commands.UpdateCategory;
using Application.Categories.DTOs;
using Application.Common.Interfaces;
using Application.Common.Utils;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Handlers
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
    {
        private readonly IApplicationDbContext _db;
        public UpdateCategoryCommandHandler(IApplicationDbContext db) => _db = db;

        public async Task<CategoryDto> Handle(UpdateCategoryCommand rq, CancellationToken ct)
        {
            var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == rq.Id, ct)
                      ?? throw new KeyNotFoundException("Category not found.");

            if (!string.IsNullOrWhiteSpace(rq.Request.Name))
                cat.Name = rq.Request.Name.Trim();

            if (rq.Request.ParentId != cat.Id) // chống tự làm parent của chính mình
                cat.ParentId = rq.Request.ParentId;

            cat.DisplayOrder = rq.Request.DisplayOrder;
            cat.IsHidden = rq.Request.IsHidden;

            if (rq.Request.Slug != null)
            {
                var newSlug = string.IsNullOrWhiteSpace(rq.Request.Slug)
                    ? SlugGenerator.Slugify(cat.Name)
                    : SlugGenerator.Slugify(rq.Request.Slug);

                if (!newSlug.Equals(cat.Slug, StringComparison.OrdinalIgnoreCase))
                {
                    var dup = await _db.Categories.AnyAsync(c => c.Slug == newSlug && c.Id != cat.Id, ct);
                    if (dup) throw new InvalidOperationException("Category slug already exists.");
                    cat.Slug = newSlug;
                }
            }

            await _db.SaveChangesAsync(ct);

            return new CategoryDto
            {
                Id = cat.Id,
                Name = cat.Name,
                Slug = cat.Slug,
                ParentId = cat.ParentId,
                DisplayOrder = cat.DisplayOrder,
                IsHidden = cat.IsHidden
            };
        }
    }
}
