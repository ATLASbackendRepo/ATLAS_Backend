using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration;

/// <summary>
/// Attendance Record Configuration.
/// </summary>
public sealed class AttendanceRecordConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.HasKey(x => x.AttendanceRecordId);
        builder.Property(x => x.DeviceHash).IsRequired().HasMaxLength(64);
        builder.HasIndex(x => new { x.AttendanceSessionId, x.StudentId }).IsUnique();
        builder.HasIndex(x => new { x.AttendanceSessionId, x.DeviceHash }).IsUnique();
        builder.HasOne(x => x.AttendanceSession).WithMany(x => x.Records)
            .HasForeignKey(x => x.AttendanceSessionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Student).WithMany()
            .HasForeignKey(x => x.StudentId).OnDelete(DeleteBehavior.Restrict);
    }
}
