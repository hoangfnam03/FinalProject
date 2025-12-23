using Application.Common.Interfaces;
using Application.Common.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity
{
    public class IdentityUserQuery : IIdentityUserQuery
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityUserQuery(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<long>> SearchUserIdsByEmailAsync(string keyword, CancellationToken ct)
        {
            keyword = keyword.Trim();

            return await _userManager.Users.AsNoTracking()
                .Where(u => u.Email != null && u.Email.Contains(keyword))
                .Select(u => u.Id)
                .ToListAsync(ct);
        }

        public async Task<Dictionary<long, IdentityUserBrief>> GetUsersByIdsAsync(IEnumerable<long> userIds, CancellationToken ct)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<long, IdentityUserBrief>();

            var list = await _userManager.Users.AsNoTracking()
                .Where(u => ids.Contains(u.Id))
                .Select(u => new IdentityUserBrief
                {
                    Id = u.Id,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber
                })
                .ToListAsync(ct);

            return list.ToDictionary(x => x.Id);
        }
    }
}
