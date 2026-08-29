// Persistence/Configurations/StudentProfileConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ATLAS.Domain.Entities;

namespace ATLAS.Persistence.Configurations
{
    public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
    {
        public void Configure(EntityTypeBuilder<StudentProfile> builder)
        {
            builder.ToTable("StudentProfiles");

            builder.HasKey(p => p.StudentProfileID);

            builder.Property(p => p.LinkedInUrl).HasMaxLength(300);
           
            builder.Property(p => p.QRCodeUrl).HasMaxLength(300);
      
            builder.Property(p => p.CreatedAt).IsRequired();

            builder.HasIndex(p => p.StudentID).IsUnique();

            builder.HasOne(p => p.Student)
                   .WithOne()
                   .HasForeignKey<StudentProfile>(p => p.StudentID)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.StudentID).IsUnique();
        }
    }
}