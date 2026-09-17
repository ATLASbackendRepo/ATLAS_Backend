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
                .HasMaxLength(200);
           
            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);
            builder.Property(u => u.MustChangePassword)
                .HasDefaultValue(false);
builder.Property(u => u.PreviousRefreshTokenHash)
                .IsRequired(false);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.Role)
          .HasConversion<string>()
          .HasMaxLength(50)
          .IsRequired();

            builder.ToTable(t =>
               t.HasCheckConstraint(
                  "CK__Users__Role",
                  "[Role] IN ('Student', 'Admin', 'SAdmin', 'Doctor', 'TeachingAssistant')"
                 ) 
                 );

                        builder.HasOne(u => u.Institution)
                   .WithMany(i => i.Users)
                   .HasForeignKey(u => u.InstitutionID)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_User_Insutition");

            builder.HasOne(u => u.Department)
                   .WithMany(d => d.Users)
                   .HasForeignKey(u => u.DepartmentId)
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("FK_Users_Departments");




        }
    }
}
