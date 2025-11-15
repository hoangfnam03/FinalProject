using System.Threading.Tasks;

namespace Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<long> CreateUserAsync(string email, string password);
        Task<string> GenerateEmailConfirmationTokenAsync(long userId);
        Task<bool> ConfirmEmailAsync(long userId, string token);
        Task<(bool Success, long UserId, string? Error)> CheckPasswordAsync(string email, string password);
        Task<bool> IsEmailConfirmedAsync(long userId);
        Task<long?> GetUserIdByEmailAsync(string email);
    }
}
