using System;

namespace Application.Members.DTOs
{
    public class MeDto
    {
        public long Id { get; set; }
        public string DisplayName { get; set; } = default!;
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }   // map từ Member.ProfilePictureLink
        public DateTime CreatedAt { get; set; }

        public bool IsModerator { get; set; }
        public bool IsAdministrator { get; set; }
        public bool IsTemporarilySuspended { get; set; }
        public DateTime? TemporarySuspensionEndAt { get; set; }
        public string? TemporarySuspensionReason { get; set; }

        public long TrustLevelId { get; set; }
        public Guid? TenantId { get; set; }
        public long UserId { get; set; }                 // khóa qua bảng Identity User
    }
}
