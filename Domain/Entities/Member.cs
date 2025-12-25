using Domain.Common;
using Domain.Common.Entities;

namespace Domain.Entities
{
    public class Member : SoftDeletableEntity, ITenantEntity
    {
        public string DisplayName { get; set; } = default!;
        public string? Bio { get; set; }
        public string? ProfilePictureLink { get; set; }
        public bool IsTemporarilySuspended { get; set; }
        public DateTime? TemporarySuspensionEndAt { get; set; }
        public string? TemporarySuspensionReason { get; set; }
        public long TrustLevelId { get; set; }
        public bool IsModerator { get; set; }
        public bool IsAdministrator { get; set; }

        public Guid UserId { get; set; } // FK tới Identity User (long key mapping – tuỳ bạn)
        public Guid? TenantId { get; set; }

        public TrustLevel? TrustLevel { get; set; }
    }
}
