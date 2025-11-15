using Domain.Common;
using Domain.Common.Entities;
using Domain.Common.Enums;

namespace Domain.Entities
{

    public class PostAttachment : AuditableEntity, ITenantEntity
    {
        public long Id { get; set; }

        public long PostId { get; set; }
        public Post Post { get; set; } = default!;

        public AttachmentType Type { get; set; }

        // Dùng cho ảnh
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public long? Size { get; set; }
        public string? Caption { get; set; }

        // Dùng cho link
        public string? LinkUrl { get; set; }
        public string? DisplayText { get; set; } // custom text hiển thị cho link

        public long? CreatedById { get; set; } // Member
        public Member? CreatedBy { get; set; }

        public Guid? TenantId { get; set; }
    }
}
