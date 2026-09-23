using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    /// <summary>
    /// Course Offering Configuration.
    /// </summary>
    public class CourseOfferingConfiguration : IEntityTypeConfiguration<CourseOffering>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<CourseOffering> builder)
        {
            builder.HasKey(co => co.CourseOfferingID);

            builder.Property(co => co.SemesterID).IsRequired();
            builder.Property(co => co.CourseID).IsRequired();

        
            builder.HasIndex(x => x.SemesterID)
                .HasDatabaseName("UQ_Course_Semester");


            builder.HasOne(co => co.Semester)
                .WithMany(s => s.CourseOfferings)
                .HasForeignKey(co => co.SemesterID)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasOne(co => co.Course)
                .WithMany(c => c.CourseOfferings)
                .HasForeignKey(co => co.CourseID)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasMany(co => co.AssessmentModules)
                .WithOne(am => am.CourseOffering)
                .HasForeignKey(am => am.CourseOfferingID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
