using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PostAttachmentConfiguration : IEntityTypeConfiguration<PostAttachment>
    {
        public void Configure(EntityTypeBuilder<PostAttachment> b)
        {
            b.ToTable("PostAttachments");
            b.HasKey(x => x.Id);

            b.Property(x => x.FileUrl).HasMaxLength(1024);
            b.Property(x => x.FileName).HasMaxLength(256);
            b.Property(x => x.ContentType).HasMaxLength(128);
            b.Property(x => x.Caption).HasMaxLength(512);

            b.Property(x => x.LinkUrl).HasMaxLength(1024);
            b.Property(x => x.DisplayText).HasMaxLength(256);

            b.HasOne(x => x.Post)
                .WithMany()
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.CreatedBy)
                .WithMany()
                .HasForeignKey(x => x.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
