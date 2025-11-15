using System;

namespace Domain.Common
{
    public interface ITenantEntity
    {
        Guid? TenantId { get; set; } // nullable để bật đa tenant sau
    }
}
