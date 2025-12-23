using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.ToTable("Reports");
            builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => new { x.TargetType, x.TargetId });
        }
    }
}
