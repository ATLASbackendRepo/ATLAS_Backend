using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {

            builder.ToTable("Courses");

            builder.HasKey(x => x.CourseID);

            builder.Property(x => x.C_Name).IsRequired()
                .HasMaxLength(100);
            builder.Property(x => x.Credit_Hours).IsRequired();
    



        }
    }

}
