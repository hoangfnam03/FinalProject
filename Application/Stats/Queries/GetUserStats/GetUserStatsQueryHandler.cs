using Application.Common.Interfaces;
using Application.Stats.DTOs;
using Application.Stats.Queries;
using Application.Stats.Queries.GetMyStats;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Stats.Queries.GetUserStats
{
    public class GetUserStatsQueryHandler : IRequestHandler<GetUserStatsQuery, UserStatsDto>
    {
        private readonly IApplicationDbContext _db;

        public GetUserStatsQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<UserStatsDto> Handle(GetUserStatsQuery request, CancellationToken ct)
        {
            // Có thể kiểm tra tồn tại user trước (404) nếu muốn
            var exists = await _db.Members.AnyAsync(m => m.Id == request.MemberId, ct);
            if (!exists) throw new KeyNotFoundException("User not found.");

            return await GetMyStatsQueryHandler.ComputeUserStats(_db, request.MemberId, ct);
        }
    }
}
