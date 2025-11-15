using System;

namespace Application.Members.DTOs
{
    public class PublicUserDto
    {
        public long Id { get; set; }
        public string DisplayName { get; set; } = default!;
        public string? ProfilePictureUrl { get; set; }   // từ ProfilePictureLink
        public DateTime JoinedAt { get; set; }           // CreatedAt của Member
        public long TrustLevelId { get; set; }
    }
}
