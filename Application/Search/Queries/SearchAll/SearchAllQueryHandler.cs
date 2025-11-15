using Application.Common.Models;
using Application.Posts.DTOs;
using Application.Search.DTOs;
using Application.Search.Queries;
using Application.Search.Queries.SearchAll;
using Application.Search.Queries.SearchPosts;
using Application.Search.Queries.SearchTags;
using Application.Search.Queries.SearchUsers;
using Application.Tags.DTOs;
using Application.Users.DTOs;
using MediatR;

namespace Application.Search.Queries.SearchAll
{
    public class SearchAllQueryHandler : IRequestHandler<SearchAllQuery, SearchAllResult>
    {
        private readonly IMediator _mediator;
        public SearchAllQueryHandler(IMediator mediator) => _mediator = mediator;

        public async Task<SearchAllResult> Handle(SearchAllQuery rq, CancellationToken ct)
        {
            var posts = await _mediator.Send(new SearchPostsQuery(rq.Q, rq.Page, rq.PageSize, "relevance"), ct);
            var users = await _mediator.Send(new SearchUsersQuery(rq.Q, rq.Page, rq.PageSize), ct);
            var tags = await _mediator.Send(new SearchTagsQuery(rq.Q, rq.Page, rq.PageSize), ct);

            return new SearchAllResult { Posts = posts, Users = users, Tags = tags };
        }

    }
}
