using Application.Categories.DTOs;
using Application.Categories.Queries.GetCategoryBySlug;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Handlers
{
    public class GetCategoryBySlugQueryHandler : IRequestHandler<GetCategoryBySlugQuery, CategoryDto>
    {
        private readonly IApplicationDbContext _db;
        public GetCategoryBySlugQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<CategoryDto> Handle(GetCategoryBySlugQuery rq, CancellationToken ct)
        {
            var c = await _db.Categories.FirstOrDefaultAsync(x => x.Slug == rq.Slug, ct)
                ?? throw new KeyNotFoundException("Category not found.");

            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ParentId = c.ParentId,
                DisplayOrder = c.DisplayOrder,
                IsHidden = c.IsHidden
            };
        }
    }
}
