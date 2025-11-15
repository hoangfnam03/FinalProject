namespace Application.Tags.DTOs
{
    public class TagDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public int UsageCount { get; set; }  // số post dùng tag này
    }
}
