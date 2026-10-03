using ATLAS.Domain.Entities;
using ATLAsDomain.Entities;
using ATLASDomain.Entities;
using ATLASDomain.Views;
using Microsoft.EntityFrameworkCore;

namespace ATLAsDCL.Data
{
    /// <summary>
    /// At LASDb Context.
    /// </summary>
    public class AtLASDbContext : DbContext
    {
        public AtLASDbContext(DbContextOptions<AtLASDbContext> options)
            : base(options)
        {
        }
        /// <summary>
        /// Audit Logs.
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }
        /// <summary>
        /// Users.
        /// </summary>
        public DbSet<User> Users { get; set; }
        /// <summary>
        /// Assessment Modules.
        /// </summary>
        public DbSet<AssessmentModule> AssessmentModules { get; set; }

        /// <summary>
        /// Students.
        /// </summary>
        public DbSet<Student> Students { get; set; }


        /// <summary>
        /// Security Events.
        /// </summary>
        public DbSet<SecurityEvent> SecurityEvents { get; set; }
        /// <summary>
        /// Departments.
        /// </summary>
        public DbSet<Department> Departments { get; set; }
        /// <summary>
        /// Institutions.
        /// </summary>
        public DbSet<Institution> Institutions { get; set; }

        /// <summary>
        /// Courses.
        /// </summary>
        public DbSet<Course> Courses { get; set; }

        /// <summary>
        /// Semesters.
        /// </summary>
        public DbSet<Semester> Semesters { get; set; }

        /// <summary>
        /// Course Offerings.
        /// </summary>
        public DbSet<CourseOffering> CourseOfferings { get; set; }

        /// <summary>
        /// Course Offering Doctors.
        /// </summary>
        public DbSet<CourseOfferingDoctor> CourseOfferingDoctors { get; set; }

        /// <summary>
        /// Course Offering TAs.
        /// </summary>
        public DbSet<CourseOfferingTA> CourseOfferingTAs { get; set; }

        /// <summary>
        /// Enrollments.
        /// </summary>
        public DbSet<Enrollment> Enrollments { get; set; }

        /// <summary>
        /// Course Grades.
        /// </summary>
        public DbSet<CourseGrade> CourseGrades { get; set; }

        /// <summary>
        /// Course Grade History. Append-only trail of every accepted grade change
        /// (MED-06).
        /// </summary>
        public DbSet<CourseGradeHistory> CourseGradeHistory { get; set; }
        /// <summary>
        /// Attendance Sessions.
        /// </summary>
        public DbSet<AttendanceSession> AttendanceSessions { get; set; } = null!;
        /// <summary>
        /// Attendance Records.
        /// </summary>
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; } = null!;
        /// <summary>
        /// Semester Student Rankings.
        /// </summary>
        public DbSet<SemesterStudentRankingView> SemesterStudentRankings { get; set; } = null!;
        /// <summary>
        /// Student Grade Models.
        /// </summary>
        public DbSet<StudentGradeModelView> StudentGradeModels { get; set; } = null!;
        /// <summary>
        /// Course Details.
        /// </summary>
        public DbSet<CourseDetailsView> CourseDetails { get; set; } = null!;


        // feature
         /// <summary>
         /// Grade Appeal Requests.
         /// </summary>
         public DbSet<GradeAppealRequest> GradeAppealRequests { get; set; } = null!;

        // Groping
        /// <summary>
        /// Course Group Messages.
        /// </summary>
        public DbSet<CourseGroupMessage> CourseGroupMessages { get; set; }
        /// <summary>
        /// Course Group Message Reads.
        /// </summary>
        public DbSet<CourseGroupMessageRead> CourseGroupMessageReads { get; set; }
        /// <summary>
        /// Notifications.
        /// </summary>
        public DbSet<Notification> Notifications { get; set; }

        // PROFILE 
        /// <summary>
        /// Student Profiles.
        /// </summary>
        public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
        /// <summary>
        /// Staff Profiles.
        /// </summary>
        public DbSet<StaffProfile> StaffProfiles => Set<StaffProfile>();

        /// <summary>
        /// On Model Creating.
        /// </summary>
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

                // The view exposes the decimal(5,2) columns of CourseGrades and
                // AssessmentModules, so the properties must not be mapped as int.
                entity.Property(x => x.studentdegree).HasPrecision(5, 2);
                entity.Property(x => x.maxDegree).HasPrecision(5, 2);
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
