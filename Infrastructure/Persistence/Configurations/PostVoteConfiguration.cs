using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class PostVoteConfiguration : IEntityTypeConfiguration<PostVote>
    {
        public void Configure(EntityTypeBuilder<PostVote> entity)
        {
            entity.ToTable("PostVotes");
            entity.Property(v => v.Value).IsRequired();
            entity.HasIndex(v => new { v.PostId, v.MemberId }).IsUnique();
            entity.HasOne(v => v.Post)
                  .WithMany()
                  .HasForeignKey(v => v.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
