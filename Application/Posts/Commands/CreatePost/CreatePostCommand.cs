using Application.Posts.DTOs;
using MediatR;

namespace Application.Posts.Commands.CreatePost
{
    public record CreatePostCommand(CreatePostRequest Request) : IRequest<PostDetailDto>;
}
