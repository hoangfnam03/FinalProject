using Application.Categories.DTOs;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Queries.GetPopularCategories
{
    public sealed record GetPopularCategoriesQuery(int Top = 3, bool IncludeHidden = false)
        : IRequest<List<PopularCategoryDto>>;

    public sealed class GetPopularCategoriesQueryHandler
        : IRequestHandler<GetPopularCategoriesQuery, List<PopularCategoryDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPopularCategoriesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PopularCategoryDto>> Handle(GetPopularCategoriesQuery request, CancellationToken ct)
        {
            var top = request.Top <= 0 ? 3 : Math.Min(request.Top, 20);

            var categoriesQuery = _context.Categories.AsNoTracking().AsQueryable();
            if (!request.IncludeHidden)
                categoriesQuery = categoriesQuery.Where(c => !c.IsHidden);

            // NOTE: giả định Post có CategoryId (long) và DbSet<Post> Posts
            var result = await categoriesQuery
                .Select(c => new PopularCategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    PostCount = _context.Posts.Count(p => p.CategoryId == c.Id)
                })
                .OrderByDescending(x => x.PostCount)
                .ThenBy(x => x.Name)
                .Take(top)
                .ToListAsync(ct);

            // (optional) Nếu bạn muốn chỉ lấy category có PostCount > 0:
            // result = result.Where(x => x.PostCount > 0).ToList();
            return result;
        }
    }
}
