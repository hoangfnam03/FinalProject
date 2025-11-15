using Application.Auth.DTOs;
using MediatR;

namespace Application.Auth.Commands.Login
{
    public record LoginCommand(LoginRequest Request) : IRequest<AuthResponse>;
}
