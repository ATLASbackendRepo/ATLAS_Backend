using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Grade Appeal Request Configuration.
/// </summary>
public class GradeAppealRequestConfiguration
    : IEntityTypeConfiguration<GradeAppealRequest>
{
    /// <summary>
    /// Configure.
    /// </summary>
    public void Configure(EntityTypeBuilder<GradeAppealRequest> builder)
    {
        builder.ToTable("GradeAppealRequests");

        builder.HasKey(x => x.RequestID);

        builder.Property(x => x.RequestID)
            .HasColumnName("RequestID");

        builder.Property(x => x.StudentID)
            .HasColumnName("StudentID")
            .IsRequired();

        builder.Property(x => x.CourseOfferingID)
            .HasColumnName("CourseOfferingID")
            .IsRequired();
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasColumnName("Status")
            .HasMaxLength(20)
            .HasDefaultValue(enGradeAppealStatus.Pending)
            .IsRequired();

        builder.Property(x => x.AssessmentModuleID)
            .HasColumnName("AssessmentModuleID");

        builder.Property(x => x.DoctorUserID)
            .HasColumnName("DoctorUserID")
            .IsRequired();

        builder.Property(x => x.CurrentGrade)
            .HasColumnName("CurrentGrade")
            .HasPrecision(5, 2);

        builder.Property(x => x.ProblemDescription)
            .HasColumnName("ProblemDescription")
            .HasColumnType("nvarchar(max)")
            .IsRequired();


        builder.Property(x => x.DoctorResponse)
            .HasColumnName("DoctorResponse")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.SubmittedAt)
            .HasColumnName("SubmittedAt")
            .HasDefaultValueSql("GETDATE()")
            .IsRequired();

        builder.Property(x => x.ReviewedAt)
            .HasColumnName("ReviewedAt");

        // MED-08: two doctors deciding the same appeal at the same time would
        // otherwise overwrite each other.
        builder.Property(x => x.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(x => x.Student)
            .WithMany()
            .HasForeignKey(x => x.StudentID)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_GradeAppealRequests_Students");

        builder.HasOne(x => x.CourseOffering)
            .WithMany()
            .HasForeignKey(x => x.CourseOfferingID)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_GradeAppealRequests_CourseOfferings");

        builder.HasOne(x => x.AssessmentModule)
            .WithMany()
            .HasForeignKey(x => x.AssessmentModuleID)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_GradeAppealRequests_AssessmentModules");

        builder.HasOne(x => x.Doctor)
            .WithMany()
            .HasForeignKey(x => x.DoctorUserID)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_GradeAppealRequests_Users");

        builder.ToTable(t =>
     t.HasCheckConstraint(
        "CK_GradeAppealRequests_Status",
            "[Status] IN ('Pending', 'UnderReview', 'NeedMoreInfo', 'Approved', 'Rejected', 'Closed')"));

        // MED-08: the "one open appeal per student and module" rule used to be a
        // check-then-insert in the repository, which two concurrent requests
        // could both pass. A filtered unique index makes the database the
        // authority: only the open states are unique, so a student may file a
        // new appeal after a previous one has been decided.
        builder.HasIndex(x => new { x.StudentID, x.AssessmentModuleID })
            .IsUnique()
            .HasFilter("[Status] IN ('Pending', 'UnderReview')")
            .HasDatabaseName("UX_GradeAppealRequests_OpenAppealPerStudentModule");
    }
}