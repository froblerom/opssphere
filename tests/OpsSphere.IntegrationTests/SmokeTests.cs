using System.Reflection;

namespace OpsSphere.IntegrationTests;

public sealed class SmokeTests
{
    [Theory]
    [InlineData("OpsSphere.Api")]
    [InlineData("OpsSphere.Application")]
    [InlineData("OpsSphere.Infrastructure")]
    [InlineData("OpsSphere.Domain")]
    public void Project_assembly_loads(string assemblyName)
    {
        var assembly = Assembly.Load(assemblyName);

        Assert.Equal(assemblyName, assembly.GetName().Name);
    }
}
