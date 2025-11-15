using Application.Common.Interfaces;
using Application.Members.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.Queries.GetMyProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, MeDto>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public GetMyProfileQueryHandler(IApplicationDbContext db, ICurrentUserService current)
        { _db = db; _current = current; }

        public async Task<MeDto> Handle(GetMyProfileQuery request, CancellationToken ct)
        {
            var meId = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            var m = await _db.Members.FirstOrDefaultAsync(x => x.Id == meId, ct)
                    ?? throw new KeyNotFoundException("Member not found.");

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
