using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLASDCL.Configuration
{
    public class AssessmentModuleConfiguration : IEntityTypeConfiguration<AssessmentModule>
    {
        public void Configure(EntityTypeBuilder<AssessmentModule> builder)
        {
            builder.HasKey(am => am.ModuleID);

            builder.Property(am => am.ModuleName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(am => am.MaxDegree)
                .HasColumnType("decimal(5,2)")
                .IsRequired();

            builder.HasOne(am => am.CourseOffering)
          .WithMany(co => co.AssessmentModules)
          .HasForeignKey(am => am.CourseOfferingID)
          .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
