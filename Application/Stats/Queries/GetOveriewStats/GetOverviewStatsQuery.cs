using Application.Stats.DTOs;
using MediatR;

namespace Application.Stats.Queries.GetOveriewStats
{
    public record GetOverviewStatsQuery(int Top = 10) : IRequest<OverviewStatsDto>;
}
