using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Notifications.Commands.MarkRead
{
    public class MarkReadNotificationsCommandHandler : IRequestHandler<MarkReadNotificationsCommand, Unit>
    {
        private readonly IApplicationDbContext _db;
        private readonly ICurrentUserService _current;

        public MarkReadNotificationsCommandHandler(IApplicationDbContext db, ICurrentUserService current)
        { _db = db; _current = current; }

        public async Task<Unit> Handle(MarkReadNotificationsCommand request, CancellationToken ct)
        {
            var me = _current.CurrentMemberId ?? throw new UnauthorizedAccessException("Login required.");
            if (request.Ids == null || request.Ids.Count == 0) return Unit.Value;

            var items = await _db.Notifications
                .Where(n => n.RecipientId == me && request.Ids.Contains(n.Id) && !n.IsRead)
                .ToListAsync(ct);

            if (items.Count > 0)
            {
                foreach (var n in items) n.IsRead = true;
                await _db.SaveChangesAsync(ct);
            }

            return Unit.Value;
        }
    }
}
