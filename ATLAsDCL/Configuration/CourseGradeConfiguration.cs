using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    public class CourseGradeConfiguration : IEntityTypeConfiguration<CourseGrade>
    {
        public void Configure(EntityTypeBuilder<CourseGrade> builder)
        {
            builder.ToTable("CourseGrades");
            builder.HasKey(cg => cg.CG_ID);

            builder.Property(cg => cg.EnrollmentID).IsRequired();
            builder.Property(cg => cg.ModuleID).IsRequired();
            builder.HasIndex(cg => new { cg.EnrollmentID, cg.ModuleID })
                .IsUnique()
                .HasDatabaseName("UX_CourseGrades_EnrollmentID_ModuleID");
      

           
            builder.Property(cg => cg.StudentDegree)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.HasOne(cg => cg.Enrollment)
                .WithMany()
                .HasForeignKey(cg => cg.EnrollmentID)
                .OnDelete(DeleteBehavior.Restrict);
            builder.ToTable("CourseGrades", table =>
            {
                table.HasCheckConstraint(
                    "CK_CourseGrades_StudentDegree_NonNegative",
                    "[StudentDegree] >= 0");
            });

            builder.HasOne(cg => cg.AssessmentModule)
                .WithMany(am => am.CourseGrades)
                .HasForeignKey(cg => cg.ModuleID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
