using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    /// <summary>
    /// Institution Configuration.
    /// </summary>
    public class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
    {
        /// <summary>
        /// Configure.
        /// </summary>
        public void Configure(EntityTypeBuilder<Institution> builder)
        {
            builder.ToTable("Institutions");

            builder.HasKey(i => i.InstitutionID);

            builder.Property(i => i.name)
                .IsRequired()
                .HasMaxLength(100);

   
        }
    }
}
