using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    /// <summary>
    /// Course Offering TAConfiguration.
    /// </summary>
    public class CourseOfferingTAConfiguration : IEntityTypeConfiguration<CourseOfferingTA>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<CourseOfferingTA> builder)
        {
            builder.HasKey(cot => cot.CourseOfferingTAID);
            builder.HasIndex(cot => new { cot.CourseOfferingID, cot.TeacherAssistID }).IsUnique();
            
         

        builder.HasOne(cot => cot.CourseOffering)
                   .WithMany(co => co.CourseOfferingTAs)
                   .HasForeignKey(cot => cot.CourseOfferingID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(cot => cot.TeachingAssistant)
                   .WithMany(ta => ta.CourseOfferingTAs)
                   .HasForeignKey(cot => cot.TeacherAssistID)
                   .OnDelete(DeleteBehavior.    Restrict);
        }
    }
}
