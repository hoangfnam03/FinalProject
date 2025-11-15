using Application.Members.DTOs;
using MediatR;

namespace Application.Members.Commands.UpdateMyProfile
{
    public record UpdateMyProfileCommand(MeUpdateRequest Request) : IRequest<MeDto>;
}
