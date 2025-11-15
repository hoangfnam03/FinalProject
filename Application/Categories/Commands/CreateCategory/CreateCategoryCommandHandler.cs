using Application.Categories.Commands.CreateCategory;
using Application.Categories.DTOs;
using Application.Common.Interfaces;
using Application.Common.Utils; // SlugGenerator
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Handlers
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
    {
        private readonly IApplicationDbContext _db;
        public CreateCategoryCommandHandler(IApplicationDbContext db) => _db = db;

        public async Task<CategoryDto> Handle(CreateCategoryCommand rq, CancellationToken ct)
        {
            var name = rq.Request.Name.Trim();
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");

            var slug = string.IsNullOrWhiteSpace(rq.Request.Slug)
                ? SlugGenerator.Slugify(name)
                : SlugGenerator.Slugify(rq.Request.Slug!);

            // Unique Slug
            var exists = await _db.Categories.AnyAsync(c => c.Slug == slug, ct);
            if (exists) throw new InvalidOperationException("Category slug already exists.");

            var cat = new Category
            {
                Name = name,
                Slug = slug,
                ParentId = rq.Request.ParentId,
                DisplayOrder = rq.Request.DisplayOrder,
                IsHidden = rq.Request.IsHidden
            };
            _db.Categories.Add(cat);
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
