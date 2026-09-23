// Persistence/Configurations/StaffProfileConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ATLAS.Domain.Entities;

namespace ATLAS.Persistence.Configurations
{
    /// <summary>
    /// Staff Profile Configuration.
    /// </summary>
    public class StaffProfileConfiguration : IEntityTypeConfiguration<StaffProfile>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<StaffProfile> builder)
        {
            builder.ToTable("StaffProfiles");

            builder.HasKey(p => p.StaffProfileID);

            builder.Property(p => p.Role).HasMaxLength(50);
            builder.Property(p => p.TeachingYears).HasMaxLength(100);
            builder.Property(p => p.CreatedAt).IsRequired();

            builder.HasOne(p => p.User)
                   .WithOne()
                   .HasForeignKey<StaffProfile>(p => p.UserID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.UserID).IsUnique();
            builder.HasOne(p => p.Department)
                 .WithMany()
                 .HasForeignKey(p => p.DepartmentID)
                 .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
