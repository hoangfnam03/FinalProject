using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> entity)
        {
            entity.ToTable("Posts");

            entity.Property(p => p.Title).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Body).IsRequired();

            entity.HasIndex(p => new { p.AuthorId, p.CreatedAt })
                  .HasDatabaseName("IX_Post_AuthorId_CreatedAt");

            entity.HasOne(p => p.Author)
                  .WithMany()
                  .HasForeignKey(p => p.AuthorId)
                  .HasPrincipalKey(m => m.UserId)   // ✅ không có <Member>
                  .OnDelete(DeleteBehavior.Restrict);


            entity.HasOne(p => p.Category)
                  .WithMany()
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(p => new { p.CategoryId, p.CreatedAt })
                  .HasDatabaseName("IX_Post_CategoryId_CreatedAt");
        }

    }
}
