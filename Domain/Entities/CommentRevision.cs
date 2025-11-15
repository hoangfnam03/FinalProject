using Domain.Common.Entities;

namespace Domain.Entities
{
    public class CommentRevision : AuditableEntity
    {
        public long Id { get; set; }

        public long CommentId { get; set; }
        public Comment Comment { get; set; } = default!;

        public string? BeforeBody { get; set; }
        public string? AfterBody { get; set; }

        public long? EditorId { get; set; }          // Member đã sửa
        public Member? Editor { get; set; }

        public string? Summary { get; set; }         // ghi chú ngắn (tuỳ chọn)
    }
}
