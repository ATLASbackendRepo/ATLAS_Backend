using ATLAsDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    /// <summary>
    /// Audit Log Configuration.
    /// </summary>
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Action)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.EntityType)
                .HasMaxLength(100);

            builder.Property(x => x.EntityId)
                .HasMaxLength(100);

            builder.Property(x => x.IpAddress)
                .HasMaxLength(45);

            builder.Property(x => x.CorrelationId)
                .HasMaxLength(100);

            builder.Property(x => x.TimestampUtc)
                .IsRequired();

           

          
        }
    }
}
