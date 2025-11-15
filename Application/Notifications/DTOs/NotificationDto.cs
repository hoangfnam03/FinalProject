using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Notifications.DTOs
{
    public class NotificationDto
    {
        public long Id { get; set; }
        public string Type { get; set; } = default!;   // tên enum
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        public long? ActorId { get; set; }
        public string? ActorName { get; set; }

        public long? PostId { get; set; }
        public string? PostTitle { get; set; }

        public long? CommentId { get; set; }
        public string? Excerpt { get; set; }

        public string? ActionUrl { get; set; } // deep-link FE (optional)
    }

}
