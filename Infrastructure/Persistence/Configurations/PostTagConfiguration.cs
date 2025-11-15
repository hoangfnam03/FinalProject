using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
    {
        public void Configure(EntityTypeBuilder<PostTag> entity)
        {
            entity.ToTable("PostTags");

            entity.HasIndex(pt => new { pt.PostId, pt.TagId }).IsUnique();

            entity.HasOne(pt => pt.Post)
                  .WithMany(p => p.PostTags)
                  .HasForeignKey(pt => pt.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pt => pt.Tag)
                  .WithMany(t => t.PostTags)
                  .HasForeignKey(pt => pt.TagId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
