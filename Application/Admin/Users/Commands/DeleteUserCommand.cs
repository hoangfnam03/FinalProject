using MediatR;

namespace Application.Admin.Users.Commands
{
    public record DeleteUserCommand(long MemberId) : IRequest;
}
