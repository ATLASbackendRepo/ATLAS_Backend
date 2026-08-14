using ATLAS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAS.Infrastructure.Persistence.Configurations
{
    public class CourseGroupMessageConfiguration : IEntityTypeConfiguration<CourseGroupMessage>
    {
        public void Configure(EntityTypeBuilder<CourseGroupMessage> builder)
        {
            builder.ToTable("CourseGroupMessages");

            builder.HasKey(m => m.MessageID);

            builder.Property(m => m.Content)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(m => m.SentAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(m => m.IsPinned)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(m => m.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            // Sender relationship -> Users
            builder.HasOne(m => m.SenderUser)
                .WithMany()
                .HasForeignKey(m => m.SenderUserID)
                .OnDelete(DeleteBehavior.Restrict);

            // Uncomment and adjust once CourseOffering entity is confirmed:
            builder.HasOne(m => m.CourseOffering)
                .WithMany()
                .HasForeignKey(m => m.CourseOfferingID)
                .OnDelete(DeleteBehavior.Cascade);

            // Helpful for "get latest messages per course offering" queries
            builder.HasIndex(m => new { m.CourseOfferingID, m.SentAt });

            // Soft-delete filter: deleted messages never come back from default queries
            builder.HasQueryFilter(m => !m.IsDeleted);
        }
    }
}       