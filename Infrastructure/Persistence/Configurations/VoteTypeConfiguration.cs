using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class VoteTypeConfiguration : IEntityTypeConfiguration<VoteType>
    {
        public void Configure(EntityTypeBuilder<VoteType> entity)
        {
            entity.ToTable("VoteTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => new { e.TenantId, e.Name }).IsUnique(false);
        }
    }
}
