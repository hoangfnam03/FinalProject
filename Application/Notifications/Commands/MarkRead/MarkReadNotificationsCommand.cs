using MediatR;

namespace Application.Notifications.Commands.MarkRead
{
    public record MarkReadNotificationsCommand(IReadOnlyCollection<long> Ids) : IRequest<Unit>;
}
