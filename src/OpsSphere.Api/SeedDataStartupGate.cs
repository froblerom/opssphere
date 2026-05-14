public static class SeedDataStartupGate
{
    public static bool ShouldRun(string environmentName, bool seedDataEnabled) =>
        seedDataEnabled &&
        (string.Equals(environmentName, "Development", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase));
}
