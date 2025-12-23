using MediatR;

namespace Application.Admin.Questions.Commands
{
    public record DeletePostCommand(long PostId) : IRequest;
}
