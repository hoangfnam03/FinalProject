using Application.Stats.DTOs;
using MediatR;

namespace Application.Stats.Queries.GetMyStats
{
    public record GetMyStatsQuery() : IRequest<UserStatsDto>;
}
