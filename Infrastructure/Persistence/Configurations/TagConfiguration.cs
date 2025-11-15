using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> entity)
        {
            entity.ToTable("Tags");
            entity.Property(t => t.Name).IsRequired().HasMaxLength(64);
            entity.Property(t => t.Slug).IsRequired().HasMaxLength(64);

            // Unique slug (per-tenant nếu muốn: thêm TenantId vào index)
            entity.HasIndex(t => t.Slug).IsUnique();
        }
    }
}
