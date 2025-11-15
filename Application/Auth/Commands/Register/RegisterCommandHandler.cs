using Application.Auth.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Unit>
    {
        private readonly IIdentityService _identity;
        private readonly IApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public RegisterCommandHandler(
            IIdentityService identity,
            IApplicationDbContext db,
            IEmailSender emailSender)
        {
            _identity = identity;
            _db = db;
            _emailSender = emailSender;
        }

        public async Task<Unit> Handle(RegisterCommand request, CancellationToken ct)
        {
            var dto = request.Request;

            // Kiểm tra trùng email qua identity
            var existedUserId = await _identity.GetUserIdByEmailAsync(dto.Email);
            if (existedUserId.HasValue)
                throw new System.InvalidOperationException("Email already registered.");

            // Tạo user (Identity)
            var userId = await _identity.CreateUserAsync(dto.Email, dto.Password);

            // Tạo member (Domain)
            var defaultTrustId = await _db.TrustLevels.OrderBy(t => t.Id).Select(t => t.Id).FirstOrDefaultAsync(ct);
            _db.Members.Add(new Member
            {
                UserId = userId,
                DisplayName = dto.DisplayName,
                IsAdministrator = false,
                IsModerator = false,
                TrustLevelId = defaultTrustId
            });
            await _db.SaveChangesAsync(ct);

            // Phát token xác thực email
            var token = await _identity.GenerateEmailConfirmationTokenAsync(userId);

            // Gửi email (demo: gửi token thuần)
            var subject = "Verify your email";
            var body = $"<p>Your verification token:</p><pre>{System.Net.WebUtility.HtmlEncode(token)}</pre>";
            await _emailSender.SendAsync(dto.Email, subject, body);

            return Unit.Value;
        }
    }
}
