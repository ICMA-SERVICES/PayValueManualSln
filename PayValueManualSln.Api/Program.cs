using PayValueManualSln.Infrastructure.Persistence;
using PayValueManualSln.Infrastructure.Shared;
using PayValueManualSln.Infrastructure.Identity;
using PayValueManualSln.Infrastructure.Identity.Seeds;
using PayValueManualSln.Core.Application;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using PayValueManualSln.Infrastructure.Identity.Models;
using Microsoft.Extensions.Configuration;
using PayValueManualSln.Domain.Entities.Setting;
using PayValueManualSln.Domain.Entities.Settings;
using PayValueManualSln.Application.DTOs;
using PayValueManualSln.Application.DTOs.Tutorial;

namespace PayValueManualSln.Api
{
	public class Program
	{
		public static async Task Main(string[] args)
		{

			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("Open", builder =>
				{
					builder.AllowAnyOrigin();
					builder.AllowAnyHeader();
					builder.AllowAnyMethod();

				});
			});

			Log.Logger = new LoggerConfiguration()
					.WriteTo.Console()
					.CreateBootstrapLogger();

				Log.Information("PayValueManual API starting..");

				builder.Host.UseSerilog((context, loggerConfiguration) =>
				{
					loggerConfiguration
					 .WriteTo.Console()
					 .ReadFrom.Configuration(context.Configuration);
                });


            // Add services to the container.

			builder.Services.AddControllers();

            // Update the problematic line to:
            builder.Services.Configure<Appsettings>(builder.Configuration.GetSection("AppSettings"));
            builder.Services.Configure<MailSettingsCredentials>(
            builder.Configuration.GetSection("MailSettingsCredentials"));
            builder.Services.AddSingleton<List<UserCredential>>();
            builder.Services.AddSingleton<JwtService>();

            builder.Services.AddHttpContextAccessor();
			builder.Services.AddHttpClient();
			builder.Services.AddIdentityInfrastructure(builder.Configuration);
           
            //services.AddHttpClient();

            //Hangfire Configure Ends
            // No changes to the existing code are needed here if the extension method is defined in the correct namespace.
            builder.Services.AddApplicationLayer();
			builder.Services.AddPersistenceInfrastructure(builder.Configuration);
			builder.Services.AddSharedInfrastructure(builder.Configuration);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PayValueManualSln.Api", Version = "v1" });

                // Enable authorization using JWT in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
                        new List<string>()
        }
    });
            });
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                try
                {
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                    await DefaultRoles.SeedAsync(userManager, roleManager);
                    await DefaultSuperAdmin.SeedAsync(userManager, roleManager);
                    await DefaultBasicUser.SeedAsync(userManager, roleManager);
                    Log.Information("Finished Seeding Default Data");
                    Log.Information("Application Starting");
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "An error occurred seeding the DB");
                }
                finally
                {
                    Log.CloseAndFlush();
                }
            }
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())	
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}
			app.UseCors(x => x
			 .SetIsOriginAllowed(origin => true)
			 .AllowAnyMethod()
			 .AllowAnyHeader()
			 .AllowCredentials());

			app.UseHttpsRedirection();

			app.UseAuthorization();
            app.UseAuthentication();

            app.MapControllers();
			app.UseSerilogRequestLogging();

			app.Run();
		}
	}
   
}