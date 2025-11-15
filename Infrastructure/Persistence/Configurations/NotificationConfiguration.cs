using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> b)
        {
            b.ToTable("Notifications");

            b.HasKey(n => n.Id);

            b.Property(n => n.Type)
                .HasConversion<int>()
                .IsRequired();

            b.Property(n => n.IsRead)
                .HasDefaultValue(false);

            b.Property(n => n.DataJson)
                .HasMaxLength(4000);

            b.HasOne(n => n.Recipient)
                .WithMany()
                .HasForeignKey(n => n.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(n => n.Actor)
                .WithMany()
                .HasForeignKey(n => n.ActorId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(n => n.Post)
                .WithMany()
                .HasForeignKey(n => n.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(n => n.Comment)
                .WithMany()
                .HasForeignKey(n => n.CommentId)
                .OnDelete(DeleteBehavior.NoAction);

            // Lọc/sort phổ biến
            b.HasIndex(n => new { n.RecipientId, n.IsRead, n.CreatedAt })
             .HasDatabaseName("IX_Notification_Recipient_Read_Created");

            b.HasIndex(n => new { n.RecipientId, n.CreatedAt })
             .HasDatabaseName("IX_Notification_Recipient_Created");
        }
    }
}
