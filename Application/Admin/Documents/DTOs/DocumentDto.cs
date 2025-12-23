namespace Application.Admin.Documents.DTOs
{
    public class DocumentDto
    {
        public long Id { get; set; }
        public string FileName { get; set; } = default!;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
