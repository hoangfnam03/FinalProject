using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common.Enums
{
    public enum NotificationType
    {
        PostCommented = 1,
        CommentReplied = 2,
        PostVoted = 3,
        CommentVoted = 4,
        Mentioned = 5,
        PostCategoryChanged = 6,
        Moderation = 7
    }
}
