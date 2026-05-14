namespace OpsSphere.Domain.Authorization;

public static class ScopeTypes
{
    public const string Region = "Region";
    public const string Country = "Country";
    public const string Account = "Account";
    public const string Campaign = "Campaign";

    public static readonly IReadOnlyList<string> All = [Region, Country, Account, Campaign];
}
