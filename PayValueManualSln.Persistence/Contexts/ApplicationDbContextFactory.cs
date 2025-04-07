using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using PayValueManualSln.Infrastructure.Persistence.Contexts;

namespace PayValueManualSln.Persistence.Contexts
{ 
  public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Build configuration manually
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../PayValueManualSln.Api")) // Adjust the path as needed
                .AddJsonFile("appsettings.json")
                .Build();
       

            // Configure DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connectionString);

            // Return an instance of your context
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

