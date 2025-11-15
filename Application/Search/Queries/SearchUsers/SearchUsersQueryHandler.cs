using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Search.Queries;
using Application.Search.Queries.SearchUsers;
using Application.Users.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Search.Queries.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, Paged<MemberSummaryDto>>
    {
        private readonly IApplicationDbContext _db;
        public SearchUsersQueryHandler(IApplicationDbContext db) => _db = db;

        public async Task<Paged<MemberSummaryDto>> Handle(SearchUsersQuery rq, CancellationToken ct)
        {
            var page = rq.Page <= 0 ? 1 : rq.Page;
            var size = rq.PageSize <= 0 ? 20 : rq.PageSize;

            var q = (rq.Q ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(q))
                return new Paged<MemberSummaryDto> { Page = page, PageSize = size, Total = 0, Items = new List<MemberSummaryDto>() };

            var users = _db.Members
                .AsNoTracking()
                .Where(m => m.DisplayName.Contains(q));

            // Simple prefix-priority ordering: startswith first
            var ordered = users
                .OrderByDescending(m => m.DisplayName.StartsWith(q))
                .ThenBy(m => m.DisplayName);

            var total = await users.CountAsync(ct);

            var items = await ordered
                .Skip((page - 1) * size)
                .Take(size)
                .Select(m => new MemberSummaryDto
                {
                    Id = m.Id,
                    DisplayName = m.DisplayName
                })
                .ToListAsync(ct);

            return new Paged<MemberSummaryDto> { Page = page, PageSize = size, Total = total, Items = items };
        }
    }
}
