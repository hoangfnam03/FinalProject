using Domain.Common.Entities;

namespace Domain.Entities.Auth
{
    public class RefreshToken : AuditableEntity
    {
        public long UserId { get; set; }
        public string Token { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? ReplacedByToken { get; set; }

        public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

        public static RefreshToken CreateForUser(long userId, int days = 14)
            => new RefreshToken
            {
                UserId = userId,
                Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
                ExpiresAt = DateTime.UtcNow.AddDays(days)
            };
    }
}
