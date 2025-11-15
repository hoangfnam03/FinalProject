using Application.Common.Interfaces;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors
{
    public class TenantInterceptor : SaveChangesInterceptor
    {
        private readonly ITenantProvider _tenant;

        public TenantInterceptor(ITenantProvider tenant) => _tenant = tenant;

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context!;
            var tenantId = _tenant.GetTenantId();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added && entry.Entity is ITenantEntity t && t.TenantId == null)
                {
                    t.TenantId = tenantId;
                }
            }

            return base.SavingChanges(eventData, result);
        }
    }
}
