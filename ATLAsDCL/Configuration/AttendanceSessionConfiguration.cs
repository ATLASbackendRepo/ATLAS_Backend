using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration;

/// <summary>
/// Attendance Session Configuration.
/// </summary>
public sealed class AttendanceSessionConfiguration : IEntityTypeConfiguration<AttendanceSession>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<AttendanceSession> builder)
    {
        builder.HasKey(x => x.AttendanceSessionId);
        builder.Property(x => x.SigningKey).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Status).HasConversion<int>();
        builder.HasIndex(x => new { x.CourseOfferingId, x.Status });

        // LOW-09: StartAsync checks for an existing active session and then
        // inserts, which is a check-then-insert race. Two instructors starting a
        // session for the same offering at the same moment both pass the check, so
        // two active windows exist and every student in that offering has two
        // valid QR codes at once. The application check stays as a friendly early
        // error; this index is what actually enforces the rule.
        //
        // The filter restricts uniqueness to the active state only, so a course
        // offering keeps its history of ended and expired sessions. Status is
        // stored as an int, so the filter uses the enum value rather than a name.
        builder.HasIndex(x => x.CourseOfferingId)
            .IsUnique()
            .HasFilter("[Status] = 1")
            .HasDatabaseName("UX_AttendanceSessions_OneActivePerOffering");
        builder.HasOne(x => x.CourseOffering).WithMany(x => x.AttendanceSessions)
            .HasForeignKey(x => x.CourseOfferingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany()
            .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
