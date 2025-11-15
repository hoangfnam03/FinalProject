using System.Collections.Generic;

namespace Application.Common.Models
{
    public class Paged<T>
    {
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int Total { get; init; }
        public List<T> Items { get; init; } = new();
    }
}
