using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayValueManualSln.Infrastructure.Shared.Services;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Shared.DapperServices;

namespace PayValueManualSln.Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration _config)
        {
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddScoped<IDapper, DapperService>(); 
        }
    }
}
