using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CommentRevisionConfiguration : IEntityTypeConfiguration<CommentRevision>
    {
        public void Configure(EntityTypeBuilder<CommentRevision> b)
        {
            b.ToTable("CommentRevisions");
            b.HasKey(x => x.Id);

            b.Property(x => x.Summary).HasMaxLength(256);

            b.HasOne(x => x.Comment)
              .WithMany() // hoặc .WithMany(c => c.Revisions) nếu bạn thêm nav ngược
              .HasForeignKey(x => x.CommentId)
              .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Editor)
              .WithMany()
              .HasForeignKey(x => x.EditorId)
              .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
