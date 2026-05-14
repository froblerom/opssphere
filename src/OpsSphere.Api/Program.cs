using OpsSphere.Application;
using OpsSphere.Infrastructure;
using OpsSphere.Infrastructure.Persistence.SeedData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

var seedDataEnabled = bool.TryParse(app.Configuration[$"{SeedDataOptions.SectionName}:Enabled"], out var parsedSeedDataEnabled) &&
    parsedSeedDataEnabled;

if (SeedDataStartupGate.ShouldRun(app.Environment.EnvironmentName, seedDataEnabled))
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<OpsSphereDataSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
