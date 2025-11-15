using Application.Common.Models;
using Application.Members.DTOs;
using MediatR;

namespace Application.Members.Queries.GetUserActivities
{
    public record GetUserActivitiesQuery(long UserId, int Page = 1, int PageSize = 20) : IRequest<Paged<ActivityDto>>;
}
