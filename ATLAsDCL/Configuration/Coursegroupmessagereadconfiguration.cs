using ATLAS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAS.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// Course Group Message Read Configuration.
    /// </summary>
    public class CourseGroupMessageReadConfiguration : IEntityTypeConfiguration<CourseGroupMessageRead>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<CourseGroupMessageRead> builder)
        {
            builder.ToTable("CourseGroupMessageReads");

            // Composite primary key: one read-record per (message, student)
            builder.HasKey(r => new { r.MessageID, r.UserID });

            builder.Property(r => r.ReadAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(r => r.Message)
                .WithMany(m => m.ReadReceipts)
                .HasForeignKey(r => r.MessageID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.user)
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
