using Application.Posts.DTOs;
using MediatR;

namespace Application.Posts.Queries.GetPostDetail
{
    public record GetPostDetailQuery(long Id) : IRequest<PostDetailDto>;
}
