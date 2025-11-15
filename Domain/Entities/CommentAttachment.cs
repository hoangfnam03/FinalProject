using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Enums;

namespace Domain.Entities
{
    // Tái dùng AttachmentType (Image|Link) nếu bạn đã tạo; nếu chưa thì thêm enum này vào file riêng.
    // public enum AttachmentType { Image = 1, Link = 2 }

    public class CommentAttachment : AuditableEntity, ITenantEntity
    {
        public long Id { get; set; }

        public long CommentId { get; set; }
        public Comment Comment { get; set; } = default!;

        public AttachmentType Type { get; set; }

        // Ảnh
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public string? Caption { get; set; }

        // Link
        public string? LinkUrl { get; set; }
        public string? DisplayText { get; set; }

        public long? CreatedById { get; set; }
        public Member? CreatedBy { get; set; }

        public Guid? TenantId { get; set; }
    }
}
