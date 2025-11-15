using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Notifications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Queries.ListNotifications
{
    public class ListNotificationsQueryHandler : IRequestHandler<ListNotificationsQuery, Paged<NotificationDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public ListNotificationsQueryHandler(IApplicationDbContext db, ICurrentUserService current)
        { _db = db; _current = current; }

        public async Task<Paged<NotificationDto>> Handle(ListNotificationsQuery rq, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");

            var page = rq.Page <= 0 ? 1 : rq.Page;
            var size = rq.PageSize <= 0 ? 20 : rq.PageSize;

            var baseQuery = _db.Notifications
                .AsNoTracking()
                .Where(n => n.RecipientId == me)
                .OrderByDescending(n => n.CreatedAt);

            var total = await baseQuery.CountAsync(ct);

            // Projection có join nhẹ để lấy ActorName, PostTitle, Excerpt
            var items = await baseQuery
                .Skip((page - 1) * size)
                .Take(size)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Type = n.Type.ToString(),
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    ActorId = n.ActorId,
                    ActorName = n.Actor != null ? n.Actor.DisplayName : null,
                    PostId = n.PostId,
                    PostTitle = n.Post != null ? n.Post.Title : null,
                    CommentId = n.CommentId,
                    Excerpt = n.DataJson,        // hoặc parse từ DataJson; MVP: để nguyên
                    ActionUrl = null             // tuỳ bạn, có thể build từ Type/PostId/CommentId
                })
                .ToListAsync(ct);

            return new Paged<NotificationDto>
            {
                Page = page,
                PageSize = size,
                Total = total,
                Items = items
            };
        }
    }
}
