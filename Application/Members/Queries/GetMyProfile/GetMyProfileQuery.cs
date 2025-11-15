using Application.Members.DTOs;
using MediatR;

namespace Application.Members.Queries.GetMyProfile
{
    public record GetMyProfileQuery() : IRequest<MeDto>;
}
