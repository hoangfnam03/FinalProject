namespace Application.Comments.DTOs
{
    public class CreateCommentRequest
    {
        public string Body { get; set; } = default!;
        public long? ParentCommentId { get; set; }
    }
}
