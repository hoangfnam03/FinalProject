using Application.Categories.DTOs;
using MediatR;

namespace Application.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(long Id, CategoryCreateUpdateRequest Request) : IRequest<CategoryDto>;
}
