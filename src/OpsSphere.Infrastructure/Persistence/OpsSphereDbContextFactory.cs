using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OpsSphere.Infrastructure.Persistence;

internal sealed class OpsSphereDbContextFactory : IDesignTimeDbContextFactory<OpsSphereDbContext>
{
    public OpsSphereDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "OpsSphere.Api"))
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets("0058d504-be53-4dae-91fc-008a06b28cf2")
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        var optionsBuilder = new DbContextOptionsBuilder<OpsSphereDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new OpsSphereDbContext(optionsBuilder.Options);
    }
}
