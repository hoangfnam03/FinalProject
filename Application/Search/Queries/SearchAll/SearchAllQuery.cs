using Application.Search.DTOs;
using MediatR;

namespace Application.Search.Queries.SearchAll
{
    public record SearchAllQuery(string Q, int Page = 1, int PageSize = 10)
        : IRequest<SearchAllResult>;
}
