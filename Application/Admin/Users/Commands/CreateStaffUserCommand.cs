using Application.Admin.Users.DTOs;
using MediatR;

namespace Application.Admin.Users.Commands
{
    public record CreateStaffUserCommand(string FullName, string Email, string Password, string Role) : IRequest<AdminUserDto>;
}
