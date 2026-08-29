using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    public class CourseOfferingTAConfiguration : IEntityTypeConfiguration<CourseOfferingTA>
    {
        public void Configure(EntityTypeBuilder<CourseOfferingTA> builder)
        {
            builder.HasKey(cot => cot.CourseOfferingTAID);
            
         

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
