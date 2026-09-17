using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration;

public sealed class AttendanceSessionConfiguration : IEntityTypeConfiguration<AttendanceSession>
{
    public void Configure(EntityTypeBuilder<AttendanceSession> builder)
    {
        builder.HasKey(x => x.AttendanceSessionId);
        builder.Property(x => x.SigningKey).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => new { x.CourseOfferingId, x.Status });
        builder.HasOne(x => x.CourseOffering).WithMany(x => x.AttendanceSessions)
            .HasForeignKey(x => x.CourseOfferingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany()
            .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
