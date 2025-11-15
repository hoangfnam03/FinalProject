using Application.Auth.DTOs;
using MediatR;

namespace Application.Auth.Commands.Register
{
    public record RegisterCommand(RegisterRequest Request) : IRequest<Unit>;
}
