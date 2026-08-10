using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATLAsDCL.DependencyInjection
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
        {
            // Provide actual registration logic here.
            return services;
        }
    }
}