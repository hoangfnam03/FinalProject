using Application.Categories.DTOs;
using Application.Categories.Queries.GetCategory;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Handlers
{
    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
    {
        private readonly IApplicationDbContext _db;
        public GetCategoriesQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<List<CategoryDto>> Handle(GetCategoriesQuery rq, CancellationToken ct)
        {
            var q = _db.Categories.AsQueryable();
            if (!rq.IncludeHidden) q = q.Where(c => !c.IsHidden);

            return await q.OrderBy(c => c.ParentId).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    ParentId = c.ParentId,
                    DisplayOrder = c.DisplayOrder,
                    IsHidden = c.IsHidden
                })
                .ToListAsync(ct);
        }
    }
}
