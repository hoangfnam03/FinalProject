using Domain.Common;
using Domain.Common.Entities;

namespace Domain.Entities
{
    public class Document : SoftDeletableEntity, ITenantEntity
    {
        public string FileName { get; set; } = default!;
        public string StoragePath { get; set; } = default!;
        public long FileSize { get; set; }
        public long UploadedByMemberId { get; set; }
        public DateTime UploadedAt { get; set; }

        public Guid? TenantId { get; set; }
    }
}
