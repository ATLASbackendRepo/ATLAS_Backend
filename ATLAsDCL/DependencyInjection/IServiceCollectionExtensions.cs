using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ATLAsDCL.DependencyInjection
{
    /// <summary>
    /// IService Collection Extensions.
    /// </summary>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Add DAL.
        /// </summary>
        public static IServiceCollection AddDAL(this IServiceCollection services, IConfiguration configuration)
        {
            // Provide actual registration logic here.
            return services;
        }
    }
}
