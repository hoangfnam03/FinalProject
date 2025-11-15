using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PostRevisionConfiguration : IEntityTypeConfiguration<PostRevision>
    {
        public void Configure(EntityTypeBuilder<PostRevision> b)
        {
            b.ToTable("PostRevisions");
            b.HasKey(x => x.Id);

            b.Property(x => x.BeforeTitle).HasMaxLength(512);
            b.Property(x => x.AfterTitle).HasMaxLength(512);
            b.Property(x => x.Summary).HasMaxLength(256);

            b.HasOne(x => x.Post)
                .WithMany() // không cần nav ngược; nếu muốn: WithMany(p => p.Revisions)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Editor)
                .WithMany()
                .HasForeignKey(x => x.EditorId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
