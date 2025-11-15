using Application.Posts.DTOs;
using MediatR;

namespace Application.Posts.Commands.VotePost
{
    public record VotePostCommand(long PostId, int Value) : IRequest<(int score, int? myVote)>;
}
