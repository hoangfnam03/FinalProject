using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> entity)
        {
            entity.ToTable("Comments");
            entity.Property(c => c.Body).IsRequired().HasMaxLength(8000);

            entity.HasIndex(c => new { c.PostId, c.CreatedAt })
                  .HasDatabaseName("IX_Comment_PostId_CreatedAt");

            entity.HasOne(c => c.Post)
                .WithMany()
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            entity.HasOne(c => c.Author)
                  .WithMany()
                  .HasForeignKey(c => c.AuthorId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
