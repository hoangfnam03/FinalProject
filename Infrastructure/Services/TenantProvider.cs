using Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace Infrastructure.Services
{
    public class TenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _http;
        public TenantProvider(IHttpContextAccessor http) => _http = http;

        public Guid? GetTenantId()
        {
            // ví dụ lấy từ header "X-Tenant-Id"
            var raw = _http.HttpContext?.Request.Headers["X-Tenant-Id"].ToString();
            if (Guid.TryParse(raw, out var id)) return id;
            return null; // single-tenant mặc định
        }
    }
}
