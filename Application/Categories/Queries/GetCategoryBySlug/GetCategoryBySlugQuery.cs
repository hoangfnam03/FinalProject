using Application.Categories.DTOs;
using MediatR;

namespace Application.Categories.Queries.GetCategoryBySlug
{
    public record GetCategoryBySlugQuery(string Slug) : IRequest<CategoryDto>;
}
