namespace Application.Categories.DTOs
{
    public class CategoryCreateUpdateRequest
    {
        public string Name { get; set; } = default!;
        public string? Slug { get; set; } // nếu null -> tự slugify từ Name
        public long? ParentId { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsHidden { get; set; } = false;
    }
}
