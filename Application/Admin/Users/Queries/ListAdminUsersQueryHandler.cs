using Application.Admin.Users.DTOs;
using Application.Common.Interfaces;
using Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Admin.Users.Queries
{
    public class ListAdminUsersQueryHandler : IRequestHandler<ListAdminUsersQuery, Paged<AdminUserDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IIdentityUserQuery _identityQuery;

        public ListAdminUsersQueryHandler(IApplicationDbContext db, IIdentityUserQuery identityQuery)
        {
            _db = db;
            _identityQuery = identityQuery;
        }

        public async Task<Paged<AdminUserDto>> Handle(ListAdminUsersQuery request, CancellationToken ct)
        {
            var kw = request.Keyword?.Trim();

            var membersQuery = _db.Members.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(kw))
            {
                var matchedUserIds = await _identityQuery.SearchUserIdsByEmailAsync(kw!, ct);

                membersQuery = membersQuery.Where(m =>
                    m.DisplayName.Contains(kw!) ||
                    matchedUserIds.Contains(m.UserId));
            }

            var total = await membersQuery.CountAsync(ct);

            var members = await membersQuery
                .OrderByDescending(m => m.CreatedAt) // nếu field khác thì đổi
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var userIds = members.Select(m => m.UserId).Distinct().ToList();
            var userDict = await _identityQuery.GetUsersByIdsAsync(userIds, ct);

            var items = members.Select(m =>
            {
                userDict.TryGetValue(m.UserId, out var u);

                return new AdminUserDto
                {
                    MemberId = m.Id,
                    Avatar = m.ProfilePictureLink,
                    FullName = m.DisplayName,
                    Email = u?.Email ?? "",
                    Phone = u?.PhoneNumber,
                    Role = m.IsAdministrator ? "Admin" : (m.IsModerator ? "Moderator" : "User"),
                    CreatedAt = m.CreatedAt
                };
            }).ToList();

            return new Paged<AdminUserDto>
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total,
                Items = items
            };
        }
    }
}
