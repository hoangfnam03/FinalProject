using Application.Members.DTOs;
using MediatR;

namespace Application.Members.Queries.GetPublicUser
{
    public record GetPublicUserQuery(long UserId) : IRequest<PublicUserDto>;
}
