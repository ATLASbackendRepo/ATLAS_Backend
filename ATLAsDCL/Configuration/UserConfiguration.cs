using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATLAsDCL.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.UserId);
            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(300);
           
            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(300);
            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);
            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
