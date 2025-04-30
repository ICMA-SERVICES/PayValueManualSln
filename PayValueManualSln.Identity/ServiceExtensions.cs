using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PayValueManualSln.Application.Interfaces;
using PayValueManualSln.Identity;
using PayValueManualSln.Infrastructure.Identity.Contexts;
using PayValueManualSln.Infrastructure.Identity.Models;
using System;
using System.Text;


namespace PayValueManualSln.Infrastructure.Identity
{
    public static class ServiceExtensions
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                //services.AddDbContext<IdentityContext>(options =>
                //    options.UseInMemoryDatabase("IdentityDb"));
            }
            else
            {
                services.AddDbContext<IdentityContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("IdentityConnection"),
                    b => b.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName)));
            }
            services.AddIdentity<ApplicationUser, Microsoft.AspNetCore.Identity.IdentityRole>().AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();       
            services.AddScoped<ICurrentUserInfoService, CurrentUserInfoService>();
        }
    }
                
}