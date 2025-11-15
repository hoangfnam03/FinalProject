using Domain.Common.Entities;
using Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Notification : AuditableEntity // có CreatedAt
    {
        public long Id { get; set; }
        public long RecipientId { get; set; }      // Member nhận
        public long? ActorId { get; set; }         // ai gây ra sự kiện
        public NotificationType Type { get; set; } // enum
        public long? PostId { get; set; }
        public long? CommentId { get; set; }
        public bool IsRead { get; set; }
        public string? DataJson { get; set; }      // mở rộng (title, excerpt, v.v.)
        public Member? Actor { get; set; }         // nav
        public Member? Recipient { get; set; }
        public Post? Post { get; set; }
        public Comment? Comment { get; set; }
    }
}
