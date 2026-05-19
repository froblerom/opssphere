using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.IntegrationTests.Persistence;

[Trait("Category", "Heavy")]
public sealed class MigrationTests
{
    [Fact]
    public async Task AllMigrations_ApplyCleanly_ToSqlServer()
    {
        await using var container = new MsSqlBuilder()
            .WithAutoRemove(true)
            .WithCleanUp(true)
            .Build();

        await container.StartAsync();

        var connectionString = container.GetConnectionString();

        var options = new DbContextOptionsBuilder<OpsSphereDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using var dbContext = new OpsSphereDbContext(options);

        await dbContext.Database.MigrateAsync();

        var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
        Assert.Empty(pendingMigrations);
    }
}
