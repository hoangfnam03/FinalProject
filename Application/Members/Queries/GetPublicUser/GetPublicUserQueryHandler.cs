using Application.Common.Interfaces;
using Application.Members.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Members.Queries.GetPublicUser
{
    public class GetPublicUserQueryHandler : IRequestHandler<GetPublicUserQuery, PublicUserDto>
    {
        private readonly IApplicationDbContext _db;
        public GetPublicUserQueryHandler(IApplicationDbContext db) { _db = db; }

        public async Task<PublicUserDto> Handle(GetPublicUserQuery request, CancellationToken ct)
        {
            var m = await _db.Members.FirstOrDefaultAsync(x => x.Id == request.UserId, ct)
                    ?? throw new KeyNotFoundException("User not found.");

            return new PublicUserDto
            {
                Id = m.Id,
                DisplayName = m.DisplayName,
                ProfilePictureUrl = m.ProfilePictureLink,
                JoinedAt = m.CreatedAt,
                TrustLevelId = m.TrustLevelId
            };
        }
    }
}
