namespace OpsSphere.Application.Tests.Architecture;

public sealed class ApplicationDependencyTests
{
    [Fact]
    public void Application_assembly_does_not_reference_api_infrastructure_or_infrastructure_packages()
    {
        var forbiddenReferences = new[]
        {
            "OpsSphere.Api",
            "OpsSphere.Infrastructure",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.Data.SqlClient",
            "Microsoft.AspNetCore.Authentication.JwtBearer",
            "Microsoft.AspNetCore.Mvc",
        };

        var referencedAssemblyNames = typeof(DependencyInjection)
            .Assembly
            .GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var forbiddenReference in forbiddenReferences)
        {
            Assert.DoesNotContain(forbiddenReference, referencedAssemblyNames);
        }
    }
}
