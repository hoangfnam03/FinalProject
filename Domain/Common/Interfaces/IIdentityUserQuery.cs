using Application.Common.Models;

namespace Application.Common.Interfaces
{
    public interface IIdentityUserQuery
    {
        Task<List<long>> SearchUserIdsByEmailAsync(string keyword, CancellationToken ct);
        Task<Dictionary<long, IdentityUserBrief>> GetUsersByIdsAsync(IEnumerable<long> userIds, CancellationToken ct);

    }
}
