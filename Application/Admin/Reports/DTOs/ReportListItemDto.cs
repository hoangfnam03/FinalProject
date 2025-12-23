namespace Application.Admin.Reports.DTOs
{
    public class ReportListItemDto
    {
        public long Id { get; set; }
        public string ReporterName { get; set; } = default!;
        public string Reason { get; set; } = default!;
        public string TargetType { get; set; } = default!; // Post | Comment
        public long TargetId { get; set; }
        public string TargetContent { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
