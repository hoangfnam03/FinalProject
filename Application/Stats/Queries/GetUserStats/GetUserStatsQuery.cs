using Application.Stats.DTOs;
using MediatR;

namespace Application.Stats.Queries.GetUserStats
{
    public record GetUserStatsQuery(long MemberId) : IRequest<UserStatsDto>;
}
