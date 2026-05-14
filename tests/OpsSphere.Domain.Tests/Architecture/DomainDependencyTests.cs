using System.Reflection;

namespace OpsSphere.Domain.Tests.Architecture;

public sealed class DomainDependencyTests
{
    [Fact]
    public void Domain_assembly_does_not_reference_outer_layers_or_infrastructure_packages()
    {
        var forbiddenReferences = new[]
        {
            "OpsSphere.Application",
            "OpsSphere.Infrastructure",
            "OpsSphere.Api",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.Data.SqlClient",
            "Microsoft.AspNetCore.Authentication.JwtBearer",
            "Microsoft.AspNetCore",
            "Microsoft.Extensions.Logging",
        };

        var referencedAssemblyNames = Assembly
            .Load("OpsSphere.Domain")
            .GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var forbiddenReference in forbiddenReferences)
        {
            Assert.DoesNotContain(forbiddenReference, referencedAssemblyNames);
        }
    }
}
