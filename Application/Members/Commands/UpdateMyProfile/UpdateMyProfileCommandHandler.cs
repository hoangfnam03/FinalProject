using Application.Common.Interfaces;
using Application.Members.Commands.UpdateMyProfile;
using Application.Members.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, MeDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public UpdateMyProfileCommandHandler(IApplicationDbContext db, ICurrentUserService current)
        { _db = db; _current = current; }

        public async Task<MeDto> Handle(UpdateMyProfileCommand request, CancellationToken ct)
        {
            var meId = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            var m = await _db.Members.FirstOrDefaultAsync(x => x.Id == meId, ct)
                    ?? throw new KeyNotFoundException("Member not found.");

            if (!string.IsNullOrWhiteSpace(request.Request.DisplayName))
                m.DisplayName = request.Request.DisplayName!.Trim();
            if (request.Request.Bio != null)
                m.Bio = request.Request.Bio;

            await _db.SaveChangesAsync(ct);

            return new MeDto
            {
                Id = m.Id,
                DisplayName = m.DisplayName,
                Bio = m.Bio,
                ProfilePictureUrl = m.ProfilePictureLink,
                CreatedAt = m.CreatedAt,
                IsModerator = m.IsModerator,
                IsAdministrator = m.IsAdministrator,
                IsTemporarilySuspended = m.IsTemporarilySuspended,
                TemporarySuspensionEndAt = m.TemporarySuspensionEndAt,
                TemporarySuspensionReason = m.TemporarySuspensionReason,
                TrustLevelId = m.TrustLevelId,
                TenantId = m.TenantId,
                UserId = m.UserId
            };
        }
    }
}
