using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TrustLevelConfiguration : IEntityTypeConfiguration<TrustLevel>
    {
        public void Configure(EntityTypeBuilder<TrustLevel> entity)
        {
            entity.ToTable("TrustLevels");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique(false);
        }
    }
}
