using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GradeAppealRequestConfiguration
    : IEntityTypeConfiguration<GradeAppealRequest>
{
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
    }
}
