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

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
  {
      services.AddDbContext<AtLASDbContext>(options =>
          options.UseSqlServer(
              configuration.GetConnectionString("DefaultConnection")));

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
        services.AddScoped<ICourseGroupMessageRepository, CourseGroupMessageRepository>();
        services.AddScoped<ICourseGroupMessageReadRepository, CourseGroupMessageReadRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        // Course access service (authorization central point)
        services.AddScoped<ATLASDomain.Services.ICourseAccessService, ATLAsDCL.Services.CourseAccessService>();
        services.AddScoped<IUnitOfWork, ATLAsDCL.UnitOfWork.UnitOfWork>();
      return services;
  }
}