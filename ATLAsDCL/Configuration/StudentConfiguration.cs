using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        // Primary Key
        builder.HasKey(x => x.StudentId);

     
        builder.Property(x => x.StudentId)
               .HasColumnName("StudentId");

        builder.Property(x => x.StudentCode)
               .HasColumnName("StudentCode")
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(x => x.UserId)
               .HasColumnName("UserId")
               .IsRequired();

        builder.Property(x => x.AcademicId)
               .HasColumnName("AcademicId")
               .IsRequired();

        builder.Property(x => x.AcademicLevel)
               .HasColumnName("AcademicLevel")
               .HasConversion<int>()
               .IsRequired();
        builder
          .ToTable("Students", tb => tb.HasTrigger("TR_Students_GenerateCode"));
        
        
        builder.Property(x => x.IsActive)
               .HasColumnName("IsActive")
               .HasDefaultValue(true);

        builder.Property(x => x.InstitutionID)
               .HasColumnName("InstitutionID")
               .IsRequired();

        builder.Property(x => x.DepartmentId)
               .HasColumnName("DepartmentId")
               .IsRequired();

        // Unique StudentCode
        builder.HasIndex(x => x.StudentCode)
               .IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.DepartmentId);
        builder.HasIndex(x => x.InstitutionID);


        // Student -> Department
        builder.HasOne(x => x.Department)
               .WithMany(d => d.Students)
               .HasForeignKey(x => x.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        // Student -> Institution
        builder.HasOne(x => x.Institution)
               .WithMany(i => i.Students)
               .HasForeignKey(x => x.InstitutionID)
               .OnDelete(DeleteBehavior.Restrict);

        // Student -> User (One-to-One)
        builder.HasOne(x => x.User)
               .WithOne(u => u.Student)
               .HasForeignKey<Student>(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}