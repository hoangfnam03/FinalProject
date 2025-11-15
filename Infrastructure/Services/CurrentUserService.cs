using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _http;

        public CurrentUserService(IHttpContextAccessor http) => _http = http;

        public long? CurrentMemberId
        {
            get
            {
                var val = _http.HttpContext?.User?.FindFirstValue("member_id");
                return long.TryParse(val, out var id) ? id : null;
            }
        }  

        public string? CurrentUserId =>
            _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
