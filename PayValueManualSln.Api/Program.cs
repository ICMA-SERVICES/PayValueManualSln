using PayValueManualSln.Infrastructure.Persistence;
using PayValueManualSln.Infrastructure.Shared;
using PayValueManualSln.Core.Application;
using Serilog;

namespace PayValueManualSln.Api
{
	public class Program
	{
		public static void Main(string[] args)
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
			// Registering Services 
			builder.Services.AddApplicationLayer();
			builder.Services.AddPersistenceInfrastructure(builder.Configuration);
			builder.Services.AddSharedInfrastructure(builder.Configuration);
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			var app = builder.Build();
			// Configure the HTTP request pipeline.
			//if (app.Environment.IsDevelopment())
			//{
				app.UseSwagger();
				app.UseSwaggerUI();
			}
			app.UseCors(x => x
			 .SetIsOriginAllowed(origin => true)
			 .AllowAnyMethod()
			 .AllowAnyHeader()
			 .AllowCredentials());
			//}

			app.UseHttpsRedirection();

			app.UseAuthorization();


			app.MapControllers();
			app.UseSerilogRequestLogging();

			app.Run();
		}
	}
}