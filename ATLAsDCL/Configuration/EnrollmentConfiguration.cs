using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;


namespace ATLAsDCL.Configuration
{
    internal class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(e => e.EnrollmentID);
            builder.Property(e => e.StudentID).IsRequired();
            builder.Property(e => e.CreatedByUserID).IsRequired();
            builder.Property(e => e.CourseOfferingID).IsRequired();
            builder.HasIndex(e => new { e.StudentID, e.CourseOfferingID })
                   .IsUnique()
                   .HasDatabaseName("UX_Enrollments_StudentID_CourseOfferingID");
            builder.HasIndex(e => e.CourseOfferingID)
                   .HasDatabaseName("IX_Enrollments_CourseOfferingID");
          
            builder.HasOne(e => e.Student)
                   .WithMany(s => s.Enrollments)
                   .HasForeignKey(e => e.StudentID)
                   .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.CreatedBySuperAdmin)
                   .WithMany(sa => sa.Enrollments)
                   .HasForeignKey(e => e.CreatedByUserID)
                   .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(e => e.CourseOffering)
                   .WithMany(co => co.Enrollments)
                   .HasForeignKey(e => e.CourseOfferingID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.CourseGrades)
        .WithOne(g => g.Enrollment)
        .HasForeignKey(g => g.EnrollmentID);
        }
    }
}   
