namespace Application.Comments.DTOs
{
    public class AddCommentLinkRequest
    {
        public string LinkUrl { get; set; } = default!;
        public string? DisplayText { get; set; }
    }
}
