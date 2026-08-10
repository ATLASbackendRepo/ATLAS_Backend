using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    internal class SemesterConfiguration : IEntityTypeConfiguration<Semester>
    {
        public void Configure(EntityTypeBuilder<Semester> builder)
        {
            builder.ToTable("Semesters");
            builder.HasKey(x => x.SemesterID);
            builder.Property(x => x.SemesterName)
                   .IsRequired()
                   .HasMaxLength(50);
            builder.Property(x => x.AcademicYear)
                   .IsRequired();

        }
    }
}   
