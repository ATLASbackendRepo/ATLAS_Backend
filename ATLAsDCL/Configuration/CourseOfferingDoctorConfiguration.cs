using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    public class CourseOfferingDoctorConfiguration : IEntityTypeConfiguration<CourseOfferingDoctor>
    {
        public void Configure(EntityTypeBuilder<CourseOfferingDoctor> builder)
        {
            builder.HasKey(cod => cod.CourseOfferingDoctorID);
            
            builder.HasIndex(cod => new { cod.CourseOfferingID, cod.DoctorID })
                   .IsUnique();
        
        builder.HasOne(cod => cod.CourseOffering)
                   .WithMany(co => co.CourseOfferingDoctors)
                   .HasForeignKey(cod => cod.CourseOfferingID)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(cod => cod.Doctor)
                   .WithMany(d => d.CourseOfferingDoctors)
                   .HasForeignKey(cod => cod.DoctorID)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
