using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    /// <summary>
    /// Security Event Configuration.
    /// </summary>
    /// <remarks>
    /// MED-11: the table is polled every 15 seconds for unalerted events, and
    /// queried per account to count recent failed logins, but neither
    /// <c>EventType</c> nor <c>AttemptedAcademicId</c> was bounded or indexed.
    /// Both were <c>nvarchar(max)</c>, so each of those queries was a full scan
    /// that could never seek into an index.
    /// </remarks>
    public class SecurityEventConfiguration : IEntityTypeConfiguration<SecurityEvent>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<SecurityEvent> builder)
        {
            builder.ToTable("SecurityEvents");

            builder.HasKey(e => e.Id);

            // The event type and the attempted account are now bounded so the
            // indexes below fit inside the key limit and stay seekable.
            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.Severity)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Low");

            builder.Property(e => e.IpAddress)
                .HasMaxLength(45);

            builder.Property(e => e.CorrelationId)
                .HasMaxLength(100);

            builder.Property(e => e.AttemptedAcademicId)
                .HasMaxLength(50);

            builder.Property(e => e.TimestampUtc)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // MED-11: serves the alert poller, which reads the events that are
            // not yet delivered and whose severity is actionable, oldest first.
            builder.HasIndex(e => new { e.IsAlerted, e.Severity, e.TimestampUtc })
                .HasDatabaseName("IX_SecurityEvents_PendingAlerts");

            // MED-11: serves the per-account failed-login backoff, which counts
            // one account's failures inside a time window.
            builder.HasIndex(e => new { e.EventType, e.AttemptedAcademicId, e.TimestampUtc })
                .HasDatabaseName("IX_SecurityEvents_FailedLoginByAccount");

            // MED-11: serves the retention job that deletes events past the
            // retention horizon.
            builder.HasIndex(e => e.TimestampUtc)
                .HasDatabaseName("IX_SecurityEvents_TimestampUtc");
        }
    }
}
