using ATLASDomain.Entities;
using ATLASDomain.Views;
using Microsoft.EntityFrameworkCore;

namespace ATLAsDCL.Data
{
    public class AtLASDbContext : DbContext
    {
        public AtLASDbContext(DbContextOptions<AtLASDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<AssessmentModule> AssessmentModules { get; set; }

        public DbSet<Student> Students { get; set; }

        

        public DbSet<Department> Departments { get; set; }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Semester> Semesters { get; set; }

        public DbSet<CourseOffering> CourseOfferings { get; set; }

        public DbSet<CourseOfferingDoctor> CourseOfferingDoctors { get; set; }

        public DbSet<CourseOfferingTA> CourseOfferingTAs { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<CourseGrade> CourseGrades { get; set; }
        public DbSet<SemesterStudentRankingView> SemesterStudentRankings { get; set; } = null!;
        public DbSet<StudentGradeModelView> StudentGradeModels { get; set; } = null!;
        public DbSet<CourseDetailsView> CourseDetails { get; set; } = null!;


        // feature
         public DbSet<GradeAppealRequest> GradeAppealRequests { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration classes automatically
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtLASDbContext).Assembly);


            modelBuilder.Entity<SemesterStudentRankingView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_SemesterStudentRanking");
            });

            modelBuilder.Entity<StudentGradeModelView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_StudentGradeMode");
            });
            modelBuilder.Entity<CourseDetailsView>(
                entity =>
            {
                entity.HasNoKey();
                entity.ToView("vw_CourseDetails");
            });


            modelBuilder.Entity<SemesterStudentRankingView>()
        .Property(x => x.TotalDegree)
        .HasPrecision(18, 2); // أو .HasColumnType("decimal(18,2)")

            modelBuilder.Entity<StudentGradeModelView>()
                .Property(x => x.StudentDegree)
                .HasPrecision(18, 2);
        }
    }


}
