using ATLAsBLL.Behaviors;
using ATLAsBLL.Behaviors.Caching;
using ATLAsBLL.Behaviour;
using ATLAsBLL.Features.CourseGrades.Commands.UpdateCourseGrade;
using ATLAsBLL.Services;
using ATLASDomain.InterFaces;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ATLAsBLL // أو اسم الـ Namespace الحقيقي عندك
{
    /// <summary>
    /// Dependency Injection.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Add Business Logic Layer.
        /// </summary>
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

          
            services.AddMemoryCache();
            services.AddValidatorsFromAssembly(
                 assembly,
                 includeInternalTypes: true);
            services.AddValidatorsFromAssemblyContaining<
                 AssignCourseGradeCommandValidator>();
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(assembly);

                
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>)); // الكاش يفرمل قبل الـ Validation والـ DB
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));      // بيلف حوالين الـ Transaction بس
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>)); // الترانساكشن آخر خطوة تؤمن الحفظ الفعلي
          
            });
            services.AddScoped<IAuditService, AuditService>();
            services.AddScoped<ISecurityEventService, SecurityEventService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            return services;
        }
    }
}
