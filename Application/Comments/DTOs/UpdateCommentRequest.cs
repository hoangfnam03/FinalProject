namespace Application.Comments.DTOs
{
    public class UpdateCommentRequest
    {
        public string Body { get; set; } = default!;
        public string? Summary { get; set; } // ghi chú cho revision (optional)
    }
}
