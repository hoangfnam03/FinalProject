using Application.Admin.Users.DTOs;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Admin.Users.Commands
{
    public class CreateStaffUserCommandHandler : IRequestHandler<CreateStaffUserCommand, AdminUserDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly IIdentityService _identity;

        public CreateStaffUserCommandHandler(IApplicationDbContext db, IIdentityService identity)
        {
            _db = db;
            _identity = identity;
        }

        public async Task<AdminUserDto> Handle(CreateStaffUserCommand request, CancellationToken ct)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var fullName = request.FullName.Trim();
            var role = (request.Role ?? "Moderator").Trim();

            // 1) Tạo identity user (lưu email + password hash ở Identity tables)
            // NOTE: cần khớp chữ ký CreateUserAsync của bạn. (xem phần "NEED YOU SEND" bên dưới)
            var userId = await _identity.CreateUserAsync(email, request.Password);

            // 2) Tạo Member profile + flags quyền
            var member = new Member
            {
                DisplayName = fullName,
                UserId = userId,
                IsModerator = role.Equals("Moderator", StringComparison.OrdinalIgnoreCase),
                IsAdministrator = role.Equals("Admin", StringComparison.OrdinalIgnoreCase),
                TrustLevelId = 1
            };

            _db.Members.Add(member);
            await _db.SaveChangesAsync(ct);

            // 3) Lấy lại brief info từ Identity qua service (KHÔNG query _db.Users)
            var u = await _identity.GetUserBriefAsync(userId, ct);

            return new AdminUserDto
            {
                MemberId = member.Id,
                Avatar = member.ProfilePictureLink,
                FullName = member.DisplayName,
                Email = u.Email ?? email,
                Phone = u.PhoneNumber,
                Role = member.IsAdministrator ? "Admin" : (member.IsModerator ? "Moderator" : "User"),
                CreatedAt = member.CreatedAt
            };
        }
    }
}
