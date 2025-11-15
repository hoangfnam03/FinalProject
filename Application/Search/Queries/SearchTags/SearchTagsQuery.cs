using Application.Common.Models;
using Application.Tags.DTOs;
using MediatR;

namespace Application.Search.Queries.SearchTags
{
    public record SearchTagsQuery(string Q, int Page = 1, int PageSize = 20)
        : IRequest<Paged<TagDto>>;
}
