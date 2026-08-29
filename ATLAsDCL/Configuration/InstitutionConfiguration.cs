using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    public class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
    {
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