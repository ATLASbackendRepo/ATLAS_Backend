using ATLAsDCL.Data;
using ATLASDomain.Interfaces.Repositories;
using ATLAsDCL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped<IGradeAppealRequestRepository,
                                  GradeAppealRequestRepository>();
        services.AddScoped<IUnitOfWork, ATLAsDCL.UnitOfWork.UnitOfWork>();
      return services;
  }
}