using Application.Admin.Users.DTOs;
using Application.Common.Models;
using MediatR;

namespace Application.Admin.Users.Queries
{
    public record ListAdminUsersQuery(string? Keyword, int Page, int PageSize) : IRequest<Paged<AdminUserDto>>;
}
