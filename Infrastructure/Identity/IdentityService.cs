using Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<long> CreateUserAsync(string email, string password)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false
            };

            var res = await _userManager.CreateAsync(user, password);
            if (!res.Succeeded)
                throw new System.InvalidOperationException(string.Join("; ", res.Errors.Select(e => e.Description)));

            return user.Id;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new System.InvalidOperationException("User not found.");
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<bool> ConfirmEmailAsync(long userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var res = await _userManager.ConfirmEmailAsync(user, token);
            return res.Succeeded;
        }

        public async Task<(bool Success, long UserId, string? Error)> CheckPasswordAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, 0, "User not found.");

            // Tùy chính sách: có thể chặn login nếu chưa xác thực email
            if (!user.EmailConfirmed)
                return (false, 0, "Email is not verified.");

            var res = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            return res.Succeeded
                ? (true, user.Id, null)
                : (false, 0, "Invalid credentials.");
        }

        public async Task<bool> IsEmailConfirmedAsync(long userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            return user != null && user.EmailConfirmed;
        }

        public async Task<long?> GetUserIdByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user?.Id;
        }
    }
}
