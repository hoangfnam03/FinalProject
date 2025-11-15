using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> entity)
        {
            entity.ToTable("RefreshTokens");
            entity.Property(x => x.Token).IsRequired().HasMaxLength(200);
            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasIndex(x => x.UserId);
        }
    }
}
