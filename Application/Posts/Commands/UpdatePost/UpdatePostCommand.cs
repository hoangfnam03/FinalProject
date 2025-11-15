using Application.Posts.DTOs;
using MediatR;

namespace Application.Posts.Commands.UpdatePost
{
    public record UpdatePostCommand(long Id, UpdatePostRequest Request) : IRequest<PostDetailDto>;
}
