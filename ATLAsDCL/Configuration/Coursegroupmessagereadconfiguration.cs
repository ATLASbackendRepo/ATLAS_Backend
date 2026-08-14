using ATLAS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAS.Infrastructure.Persistence.Configurations
{
    public class CourseGroupMessageReadConfiguration : IEntityTypeConfiguration<CourseGroupMessageRead>
    {
        public void Configure(EntityTypeBuilder<CourseGroupMessageRead> builder)
        {
            builder.ToTable("CourseGroupMessageReads");

            // Composite primary key: one read-record per (message, student)
            builder.HasKey(r => new { r.MessageID, r.StudentID });

            builder.Property(r => r.ReadAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(r => r.Message)
                .WithMany(m => m.ReadReceipts)
                .HasForeignKey(r => r.MessageID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}