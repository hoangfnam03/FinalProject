using Application.Common.Models;
using Application.Notifications.DTOs;
using MediatR;

namespace Application.Notifications.Queries.ListNotifications
{
    public record ListNotificationsQuery(int Page = 1, int PageSize = 20) : IRequest<Paged<NotificationDto>>;
}
