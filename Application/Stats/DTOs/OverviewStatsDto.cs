using System.Collections.Generic;

namespace Application.Stats.DTOs
{
    public class OverviewStatsDto
    {
        public long TotalQuestions { get; set; }
        public long TotalComments { get; set; }
        public long TotalVotes { get; set; } // postVotes + commentVotes
        public long TotalUsers { get; set; }
        public List<TopTagDto> TopTags { get; set; } = new();
    }

    public class TopTagDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = default!;
        public string Slug { get; set; } = default!;
        public int Count { get; set; } // số bài dùng tag này
    }
}
