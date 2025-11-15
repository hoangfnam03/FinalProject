namespace Application.Categories.DTOs
{
    public class CategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public long? ParentId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsHidden { get; set; }
    }
}
