using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    /// <summary>
    /// Course Grade Configuration.
    /// </summary>
    public class CourseGradeConfiguration : IEntityTypeConfiguration<CourseGrade>
    {
        /// <summary>
        /// Configure.
        /// </summary>
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

            // MED-06: the database maintains the row version and EF adds it to
            // the WHERE clause of every update, so a concurrent second write is
            // rejected with a concurrency conflict instead of overwriting the
            // first one.
            builder.Property(cg => cg.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

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

            builder.HasMany(cg => cg.History)
                .WithOne(h => h.CourseGrade)
                .HasForeignKey(h => h.CG_ID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    /// <summary>
    /// Course Grade History Configuration.
    /// </summary>
    public class CourseGradeHistoryConfiguration
        : IEntityTypeConfiguration<CourseGradeHistory>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<CourseGradeHistory> builder)
        {
            builder.ToTable("CourseGradeHistory");

            builder.HasKey(h => h.HistoryId);

            builder.Property(h => h.PreviousDegree)
                .HasColumnType("decimal(5,2)");
            builder.Property(h => h.NewDegree)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.Property(h => h.ChangedAtUtc)
                .HasColumnName("ChangedAtUtc")
                .HasDefaultValueSql("GETDATE()")
                .IsRequired();

            // The history is read per grade, most recent first.
            builder.HasIndex(h => new { h.CG_ID, h.ChangedAtUtc })
                .HasDatabaseName("IX_CourseGradeHistory_CG_ID");
        }
    }
}
