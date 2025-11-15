using Application.Common.Interfaces;
using Domain.Common.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Linq;

namespace Infrastructure.Persistence.Interceptors
{
    public class AuditingInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _current;

        public AuditingInterceptor(ICurrentUserService current)
        {
            _current = current;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            var context = eventData.Context!;
            var now = DateTime.UtcNow;
            var currentMemberId = _current.CurrentMemberId;

            foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedByMemberId = currentMemberId;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedAt = now;
                    entry.Entity.LastModifiedByMemberId = currentMemberId;
                }
            }

            return base.SavingChanges(eventData, result);
        }
    }
}
