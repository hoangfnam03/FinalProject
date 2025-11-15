using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class CommentVoteConfiguration : IEntityTypeConfiguration<CommentVote>
    {
        public void Configure(EntityTypeBuilder<CommentVote> entity)
        {
            entity.ToTable("CommentVotes");
            entity.Property(v => v.Value).IsRequired();

            entity.HasIndex(v => new { v.CommentId, v.MemberId }).IsUnique();

            entity.HasOne(v => v.Comment)
                  .WithMany()
                  .HasForeignKey(v => v.CommentId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
