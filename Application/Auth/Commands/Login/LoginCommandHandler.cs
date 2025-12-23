using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Entities.Auth;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly IIdentityService _identity;
        private readonly IJwtTokenGenerator _jwt;
        private readonly IApplicationDbContext _db;

        public LoginCommandHandler(
            IIdentityService identity,
            IJwtTokenGenerator jwt,
            IApplicationDbContext db)
        {
            _identity = identity;
            _jwt = jwt;
            _db = db;
        }

        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
        {
            var dto = request.Request;

            var (ok, userId, error) = await _identity.CheckPasswordAsync(dto.Email, dto.Password);
            if (!ok) throw new InvalidOperationException(error ?? "Invalid credentials.");

            // Phát JWT
            var (access, exp) = _jwt.Generate(userId, dto.Email);

            // Tạo refresh token
            var refresh = RefreshToken.CreateForUser(userId);
            _db.RefreshTokens.Add(refresh);   // << dùng property
            await _db.SaveChangesAsync(ct);

            return new AuthResponse
            {
                AccessToken = access,
                ExpiresIn = exp,
                RefreshToken = refresh.Token
            };
        }
    }
}
