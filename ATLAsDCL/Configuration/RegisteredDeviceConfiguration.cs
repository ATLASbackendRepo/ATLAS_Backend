using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration;

/// <summary>
/// Registered Device Configuration.
/// </summary>
public sealed class RegisteredDeviceConfiguration : IEntityTypeConfiguration<RegisteredDevice>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<RegisteredDevice> builder)
    {
        builder.HasKey(x => x.RegisteredDeviceId);
        builder.Property(x => x.DeviceId).IsRequired().HasMaxLength(64);
        builder.Property(x => x.DeviceIdHash).IsRequired().HasMaxLength(64);
        builder.Property(x => x.DeviceSecret).IsRequired().HasMaxLength(128);
        builder.Property(x => x.DeviceName).HasMaxLength(100);

        // MED-17: a device identifier may belong to exactly one account. Without
        // this the pairing endpoint would let a second account claim a device that
        // is already paired, and the per-session device rule could be split across
        // two owners.
        builder.HasIndex(x => x.DeviceIdHash).IsUnique();
        builder.HasIndex(x => x.UserId);

        builder.HasOne(x => x.User).WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
