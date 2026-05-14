namespace OpsSphere.IntegrationTests.Persistence;

public sealed class SeedDataStartupGateTests
{
    [Theory]
    [InlineData("Development", true, true)]
    [InlineData("Testing", true, true)]
    [InlineData("Production", true, false)]
    [InlineData("Staging", true, false)]
    [InlineData("Development", false, false)]
    [InlineData("Testing", false, false)]
    public void ShouldRun_ShouldRequireDevelopmentOrTestingAndEnabledFlag(
        string environmentName,
        bool seedDataEnabled,
        bool expected)
    {
        Assert.Equal(expected, global::SeedDataStartupGate.ShouldRun(environmentName, seedDataEnabled));
    }
}
