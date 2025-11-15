using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Queries.GetUnreadCount
{
    public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, int>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public GetUnreadCountQueryHandler(IApplicationDbContext db, ICurrentUserService current)
        { _db = db; _current = current; }

        public async Task<int> Handle(GetUnreadCountQuery request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            return await _db.Notifications
                .AsNoTracking()
                .CountAsync(n => n.RecipientId == me && !n.IsRead, ct);
        }
    }
}
