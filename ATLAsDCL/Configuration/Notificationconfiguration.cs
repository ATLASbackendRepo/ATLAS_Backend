using ATLAS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAS.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.NotificationID);

            builder.Property(n => n.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Body)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Recipient -> Users (Restrict so deleting a message doesn't cascade-delete a user)
            builder.HasOne(n => n.RecipientUser)
                .WithMany()
                .HasForeignKey(n => n.RecipientUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // RelatedMessage is optional (nullable FK) since Notifications is a generic table
            builder.HasOne(n => n.RelatedMessage)
                .WithMany(m => m.Notifications)
                .HasForeignKey(n => n.RelatedMessageID)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

        }
    }
}