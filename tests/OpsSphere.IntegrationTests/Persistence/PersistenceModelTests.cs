using Microsoft.EntityFrameworkCore;
using OpsSphere.Domain.Entities;
using OpsSphere.Infrastructure.Persistence;

namespace OpsSphere.IntegrationTests.Persistence;

public sealed class PersistenceModelTests
{
    private static OpsSphereDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<OpsSphereDbContext>()
            .UseSqlServer("Server=localhost;Database=Test;User Id=sa;Password=placeholder;TrustServerCertificate=True;")
            .Options;
        return new OpsSphereDbContext(options);
    }

    [Fact]
    public void DbContext_model_contains_Users_table()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));
        Assert.NotNull(entityType);
        Assert.Equal("Users", entityType.GetTableName());
    }

    [Fact]
    public void DbContext_model_contains_Tickets_table()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Ticket));
        Assert.NotNull(entityType);
        Assert.Equal("Tickets", entityType.GetTableName());
    }

    [Fact]
    public void UserRoles_has_composite_primary_key()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(UserRole));
        Assert.NotNull(entityType);
        var pk = entityType.FindPrimaryKey();
        Assert.NotNull(pk);
        var keyProperties = pk.Properties.Select(p => p.Name).ToList();
        Assert.Contains("UserId", keyProperties);
        Assert.Contains("RoleId", keyProperties);
        Assert.Equal(2, keyProperties.Count);
    }

    [Fact]
    public void RolePermissions_has_composite_primary_key()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(RolePermission));
        Assert.NotNull(entityType);
        var pk = entityType.FindPrimaryKey();
        Assert.NotNull(pk);
        var keyProperties = pk.Properties.Select(p => p.Name).ToList();
        Assert.Contains("RoleId", keyProperties);
        Assert.Contains("PermissionId", keyProperties);
        Assert.Equal(2, keyProperties.Count);
    }

    [Fact]
    public void Users_Email_has_unique_index()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));
        Assert.NotNull(entityType);
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.GetDatabaseName() == "UQ_Users_Email");
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Roles_Name_has_unique_index()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Role));
        Assert.NotNull(entityType);
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.GetDatabaseName() == "UQ_Roles_Name");
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void Permissions_Code_has_unique_index()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(Permission));
        Assert.NotNull(entityType);
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.GetDatabaseName() == "UQ_Permissions_Code");
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void DbContext_model_registers_all_twenty_entity_types()
    {
        using var context = CreateContext();
        var entityTypes = context.Model.GetEntityTypes().Select(e => e.ClrType).ToHashSet();
        var expected = new[]
        {
            typeof(User), typeof(Role), typeof(Permission), typeof(UserRole), typeof(RolePermission),
            typeof(UserScope), typeof(Region), typeof(Country), typeof(Account), typeof(Campaign),
            typeof(Customer), typeof(Ticket), typeof(TicketComment), typeof(TicketAssignment),
            typeof(TicketStatusHistory), typeof(TicketEscalation), typeof(TicketResolution),
            typeof(SlaPolicy), typeof(TicketSlaState), typeof(AuditLog)
        };
        foreach (var type in expected)
        {
            Assert.Contains(type, entityTypes);
        }
    }
}
