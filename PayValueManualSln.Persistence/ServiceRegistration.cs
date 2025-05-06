using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Infrastructure.Persistence.Contexts;
using PayValueManualSln.Infrastructure.Persistence.Repository;
using PayValueManualSln.Persistence.Repositories;
using PayValueManualSln.Persistence.Services;
using PayValueManualSln.Shared.Services;

namespace PayValueManualSln.Infrastructure.Persistence
{
	public static class ServiceRegistration
	{
		public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<IcmaCollectionContext>(options =>
                   options.UseInMemoryDatabase("IcmaCollectionDb"));

            #region Repositories
            services.AddTransient(typeof(IGenericRepositoryAsync<>), typeof(GenericRepositoryAsync<>));
			#endregion
			services.AddScoped<IEntityManager, EntityMangerAsync>();
			services.AddTransient<RateServices>();
			services.AddTransient<IAuditRepository, AuditRepository>();
            services.AddScoped<IMenuRepository, MenuRepository>();



        }
    }
}
