using Application.Categories.DTOs;
using MediatR;

namespace Application.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand(CategoryCreateUpdateRequest Request) : IRequest<CategoryDto>;
}
