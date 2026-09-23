using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    /// <summary>
    /// Department Configuration.
    /// </summary>
    internal class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(d => d.DepartmentId);
         
            builder.Property(d => d.D_Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(d => d.Institution)
              .WithMany(i => i.Departments)
              .HasForeignKey(d => d.InstitutionID);

        }
    }
}
