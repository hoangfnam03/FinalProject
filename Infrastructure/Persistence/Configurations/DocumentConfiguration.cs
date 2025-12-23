using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");
            builder.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            builder.Property(x => x.StoragePath).HasMaxLength(1024).IsRequired();
            builder.HasIndex(x => x.UploadedAt);
        }
    }
}
