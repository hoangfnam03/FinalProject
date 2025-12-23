using Application.Admin.Questions.DTOs;
using Application.Common.Models;
using MediatR;

namespace Application.Admin.Questions.Queries
{
    public record ListAdminPostsQuery(
        string? Keyword,
        string? Status,
        long? CategoryId,
        int Page,
        int PageSize
    ) : IRequest<Paged<AdminPostListItemDto>>;
}
