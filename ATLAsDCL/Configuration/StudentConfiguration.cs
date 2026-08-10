using ATLASDomain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students");

        builder.HasKey(x => x.StudentId);

        builder.Property(x => x.StudentCode).IsRequired()
               .HasMaxLength(20);
        builder.HasIndex(x => x.StudentCode).IsUnique();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.AcademicId).IsRequired();
        builder.Property(x => x.DepartmentId).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.Property(x => x.AcademicLevel)
               .HasConversion<int>() 
               .IsRequired();

        builder.HasOne(x => x.Department)
               .WithMany(d => d.Students)
               .HasForeignKey(x => x.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);
        // Relationship: Student -> Department (many students per department)
        builder.HasOne(s => s.Department)
               .WithMany(d => d.Students)
               .HasForeignKey(s => s.DepartmentId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Student -> User (one-to-one)
        builder.HasOne(x => x.User)
               .WithOne(u => u.Student)
               .HasForeignKey<Student>(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}