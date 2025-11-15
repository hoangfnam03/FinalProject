using MediatR;

namespace Application.Notifications.Queries.GetUnreadCount
{
    public record GetUnreadCountQuery() : IRequest<int>;
}
