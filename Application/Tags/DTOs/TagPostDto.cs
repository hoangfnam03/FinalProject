namespace Application.Tags.DTOs
{
    public class TagPostDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = default!;
        public List<string> Tags { get; set; } = new();
    }
}
