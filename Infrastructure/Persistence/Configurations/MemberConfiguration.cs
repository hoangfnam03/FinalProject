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

            // >>> Thêm FK thật tới AspNetUsers(Id)
            entity.HasOne<ApplicationUser>()      // không cần navigation trên Member
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict); // chặn xoá User khi còn Member
        }
    }
}
