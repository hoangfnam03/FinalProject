using System;

namespace Application.Common.Interfaces
{
    public interface ITenantProvider
    {
        Guid? GetTenantId(); // có thể null nếu single-tenant hoặc chưa resolve
    }
}
