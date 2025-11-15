using Domain.Common.Entities;

namespace Domain.Entities
{
    public class PostRevision : AuditableEntity // có CreatedAt, CreatedBy… nếu AuditableEntity của bạn có
    {
        public long Id { get; set; }

        public long PostId { get; set; }
        public Post Post { get; set; } = default!;

        // Trạng thái trước & sau
        public string? BeforeTitle { get; set; }
        public string? AfterTitle { get; set; }
        public string? BeforeBody { get; set; }
        public string? AfterBody { get; set; }

        // Ai sửa (Member)
        public long? EditorId { get; set; }
        public Member? Editor { get; set; }

        public string? Summary { get; set; } // tuỳ chọn: ghi chú ngắn
    }
}
