namespace Application.Posts.DTOs
{
    public class AddLinkRequest
    {
        public string LinkUrl { get; set; } = default!;
        public string? DisplayText { get; set; } // custom text hiển thị
    }
}
