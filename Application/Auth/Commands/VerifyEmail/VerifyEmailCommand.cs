using Application.Auth.DTOs;
using MediatR;

namespace Application.Auth.Commands.VerifyEmail
{
    public record VerifyEmailCommand(VerifyEmailRequest Request) : IRequest<Unit>;
}
