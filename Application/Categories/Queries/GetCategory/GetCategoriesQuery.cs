using Application.Categories.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Application.Categories.Queries.GetCategory
{
    public record GetCategoriesQuery(bool IncludeHidden = false) : IRequest<List<CategoryDto>>;
}
