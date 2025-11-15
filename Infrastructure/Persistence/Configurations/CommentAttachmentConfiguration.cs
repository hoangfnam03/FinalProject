using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CommentAttachmentConfiguration : IEntityTypeConfiguration<CommentAttachment>
    {
        public void Configure(EntityTypeBuilder<CommentAttachment> b)
        {
            b.ToTable("CommentAttachments");
            b.HasKey(x => x.Id);

            b.Property(x => x.FileUrl).HasMaxLength(1024);
            b.Property(x => x.FileName).HasMaxLength(256);
            b.Property(x => x.ContentType).HasMaxLength(128);
            b.Property(x => x.Caption).HasMaxLength(512);
            b.Property(x => x.LinkUrl).HasMaxLength(1024);
            b.Property(x => x.DisplayText).HasMaxLength(256);

            b.HasOne(x => x.Comment)
                .WithMany()
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
