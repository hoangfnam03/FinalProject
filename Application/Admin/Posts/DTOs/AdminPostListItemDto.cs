namespace Application.Admin.Questions.DTOs
{
    public class AdminPostListItemDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = default!;
        public string AuthorName { get; set; } = default!;
        public string CategoryName { get; set; } = default!;
        public long CategoryId { get; set; }
        public int Comments { get; set; }
        public string Status { get; set; } = "Unanswered"; // Answered | Unanswered
        public DateTime CreatedAt { get; set; }
    }
}
