using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLASDCL.Configuration
{
    /// <summary>
    /// Assessment Module Configuration.
    /// </summary>
    public class AssessmentModuleConfiguration : IEntityTypeConfiguration<AssessmentModule>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<AssessmentModule> builder)
        {
            builder.HasKey(am => am.ModuleID);

            builder.Property(am => am.ModuleName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(am => am.MaxDegree)
                .HasColumnType("decimal(5,2)")
                .IsRequired();


            builder.ToTable("AssessmentModules", table =>
            {
                table.HasCheckConstraint(
                    "CK_AssessmentModules_MaxDegree_Range",
                    "[MaxDegree] >= 1 AND [MaxDegree] <= 40");
            });

            builder.HasOne(am => am.CourseOffering)
          .WithMany(co => co.AssessmentModules)
          .HasForeignKey(am => am.CourseOfferingID)
          .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
