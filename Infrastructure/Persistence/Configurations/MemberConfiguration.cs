using Domain.Entities;
using Infrastructure.Identity; // <-- thêm dòng này để dùng ApplicationUser
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> entity)
        {
            entity.ToTable("Members");

            entity.Property(e => e.DisplayName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(e => e.DisplayName);
            entity.HasIndex(e => new { e.TenantId, e.DisplayName });

            entity.HasOne(e => e.TrustLevel)
                .WithMany()
                .HasForeignKey(e => e.TrustLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // 1-1 với User qua UserId (Identity)
            entity.HasIndex(e => e.UserId).IsUnique();

            // quan trọng: cho phép dùng UserId làm principal key
            entity.HasAlternateKey(e => e.UserId);

            // FK thật tới AspNetUsers(Id)
            entity.HasOne<ApplicationUser>()
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
