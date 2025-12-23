namespace Application.Categories.DTOs
{
    public class PopularCategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public int PostCount { get; set; } // số câu hỏi/post trong category
    }
}
