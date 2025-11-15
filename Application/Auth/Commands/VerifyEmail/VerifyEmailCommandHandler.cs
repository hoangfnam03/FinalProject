using Application.Common.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
    {
        private readonly IIdentityService _identity;
        public VerifyEmailCommandHandler(IIdentityService identity) => _identity = identity;

        public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken ct)
        {
            var dto = request.Request;
            var userId = await _identity.GetUserIdByEmailAsync(dto.Email)
                         ?? throw new System.InvalidOperationException("User not found.");

            var ok = await _identity.ConfirmEmailAsync(userId, dto.Token);
            if (!ok) throw new System.InvalidOperationException("Invalid or expired token.");

            return Unit.Value;
        }
    }
}
