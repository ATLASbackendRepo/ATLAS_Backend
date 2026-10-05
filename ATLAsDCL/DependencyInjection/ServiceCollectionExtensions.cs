using ATLAsDCL.Data;
using ATLASDomain.Interfaces.Repositories;
using ATLAsDCL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ATLAsDCL.Repositories;
using ATLAS.Persistence.Repositories;
using ATLASDomain.InterFaces.Repositories;

namespace ATLAsDCL.DependencyInjection;

/// <summary>
/// Service Collection Extensions.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add DAL.
    /// </summary>
    public static IServiceCollection AddDAL(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection");

        // LOW-01: the connection string was a literal sa password committed to
        // appsettings.json, and is now supplied by user-secrets, an environment
        // variable or the compose .env file. Failing here means a missing secret
        // produces one clear message naming the fix, instead of an obscure SQL
        // error much later when the first query opens a connection.
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No database connection string is configured. Set the " +
                "ConnectionStrings:DefaultConnection value through user-secrets " +
                "(dotnet user-secrets set ConnectionStrings:DefaultConnection \"<value>\"), " +
                "the ConnectionStrings__DefaultConnection environment variable, or the .env " +
                "file used by docker-compose. See .env.example. " +
                "It is intentionally not committed to appsettings.json.");
        }

        // DbContext used by repositories/services.
        // The options are a singleton because the factory below is a singleton:
        // a scoped DbContextOptions cannot be consumed from the root provider,
        // which is what made the Development environment fail service scope
        // validation at startup. The options are immutable configuration, so a
        // single instance is shared by every scope.
        services.AddDbContext<AtLASDbContext>(
            options => options.UseSqlServer(connectionString),
            optionsLifetime: ServiceLifetime.Singleton);

        // DbContextFactory used by Audit/Security services
        services.AddDbContextFactory<AtLASDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAssessmentModuleRepository, AssessmentModuleRepository>();
        services.AddScoped<ICourseGradeRepository, CourseGradeRepository>();
        services.AddScoped<ICourseOfferingDoctorRepository, CourseOfferingDoctorRepository>();
        services.AddScoped<ICourseOfferingRepository, CourseOfferingRepository>();
        services.AddScoped<ICourseOfferingTARepository, CourseOfferingTARepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IStaffProfileRepository, StaffProfileRepository>();
        services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
        services.AddScoped<IinstitutionRepository, institutionRepository>();

        services.AddScoped<IGradeAppealRequestRepository,
            GradeAppealRequestRepository>();

        services.AddScoped<ICourseGroupMessageRepository,
            CourseGroupMessageRepository>();

        services.AddScoped<ICourseGroupMessageReadRepository,
            CourseGroupMessageReadRepository>();

        services.AddScoped<INotificationRepository,
            NotificationRepository>();

        services.AddScoped<
            ATLASDomain.Services.ICourseAccessService,
            ATLAsDCL.Services.CourseAccessService>();

        services.AddScoped<IUnitOfWork,
            ATLAsDCL.UnitOfWork.UnitOfWork>();

        return services;
    }
}
