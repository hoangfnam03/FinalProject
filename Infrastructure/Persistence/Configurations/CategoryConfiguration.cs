using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> entity)
        {
            entity.ToTable("Categories");
            entity.Property(c => c.Name).IsRequired().HasMaxLength(120);
            entity.Property(c => c.Slug).IsRequired().HasMaxLength(128);

            entity.HasIndex(c => c.Slug).IsUnique(); // hoặc Unique (TenantId, Slug) nếu multi-tenant
            entity.HasIndex(c => new { c.ParentId, c.DisplayOrder })
                  .HasDatabaseName("IX_Category_ParentId_DisplayOrder");

            entity.HasOne(c => c.Parent)
                  .WithMany()
                  .HasForeignKey(c => c.ParentId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
