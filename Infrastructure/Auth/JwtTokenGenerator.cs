using Application.Common.Interfaces;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Auth
{
    public class JwtSettings
    {
        public string Issuer { get; set; } = "QnA";
        public string Audience { get; set; } = "QnAClients";
        public string Key { get; set; } = default!;
        public int AccessTokenMinutes { get; set; } = 15;
    }

    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtSettings _settings;
        private readonly IApplicationDbContext _db;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public JwtTokenGenerator(
            IOptions<JwtSettings> options,
            IApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _settings = options.Value;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public (string AccessToken, int ExpiresInSeconds) Generate(
            long userId,
            string email,
            IEnumerable<(string type, string value)>? extraClaims = null)
        {
            // Map Member.Id theo UserId
            var member = _db.Members
                .Where(m => m.UserId == userId)
                .Select(m => new { m.Id, m.IsAdministrator, m.IsModerator })
                .FirstOrDefault();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (member != null)
            {
                claims.Add(new Claim("member_id", member.Id.ToString()));

                // nếu bạn vẫn muốn dùng is_admin check (hoặc debug)
                if (member.IsAdministrator)
                    claims.Add(new Claim("is_admin", "true"));
            }

            // ======= ADD ROLE + PERMISSION CLAIMS =======
            // Vì method Generate đang sync, mình dùng GetAwaiter().GetResult() để tránh phải đổi interface.
            var user = _userManager.Users.AsNoTracking()
                .FirstOrDefault(u => u.Id == userId);

            if (user != null)
            {
                // user claims (nếu có)
                var userClaims = _userManager.GetClaimsAsync(user).GetAwaiter().GetResult();
                claims.AddRange(userClaims);

                // roles + role claims
                var roles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
                foreach (var roleName in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));

                    var role = _roleManager.FindByNameAsync(roleName).GetAwaiter().GetResult();
                    if (role == null) continue;

                    var roleClaims = _roleManager.GetClaimsAsync(role).GetAwaiter().GetResult();

                    // add permission claims (đúng type "permission" như policy bạn cấu hình)
                    foreach (var rc in roleClaims.Where(c => c.Type == "permission"))
                        claims.Add(rc);
                }
            }

            // extra claims from caller
            if (extraClaims != null)
                claims.AddRange(extraClaims.Select(c => new Claim(c.type, c.value)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokenMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expires,
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return (jwt, _settings.AccessTokenMinutes * 60);
        }
    }
}
