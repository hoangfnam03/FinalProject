namespace Application.Stats.DTOs
{
    public class UserStatsDto
    {
        public int Questions { get; set; }
        public int Comments { get; set; }
        public int Votes { get; set; }       // số lần user đã vote (post+comment)
        public int Reputation { get; set; }      // tổng up/downvote nhận được trên bài & bình luận của user
    }
}
