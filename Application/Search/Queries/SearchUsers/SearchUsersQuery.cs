using Application.Common.Models;
using Application.Users.DTOs;
using MediatR;

namespace Application.Search.Queries.SearchUsers
{
    public record SearchUsersQuery(string Q, int Page = 1, int PageSize = 20)
        : IRequest<Paged<MemberSummaryDto>>;
}
