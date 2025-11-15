using Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Linq;

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
        private readonly IApplicationDbContext _db;  // <-- cần để lookup Member.Id theo UserId

        public JwtTokenGenerator(IOptions<JwtSettings> options, IApplicationDbContext db)
        {
            _settings = options.Value;
            _db = db;
        }

        public (string AccessToken, int ExpiresInSeconds) Generate(
            long userId,
            string email,
            IEnumerable<(string type, string value)>? extraClaims = null)
        {
            // Tìm Member.Id map 1–1 với UserId (AspNetUsers.Id)
            var memberId = _db.Members
                              .Where(m => m.UserId == userId)
                              .Select(m => (long?)m.Id)
                              .FirstOrDefault();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // tiện để revoke theo jti nếu muốn
            };

            if (memberId.HasValue)
                claims.Add(new Claim("member_id", memberId.Value.ToString()));

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
