namespace OpsSphere.Domain.Authorization;

public static class Roles
{
    public const string Admin = "Admin";
    public const string OperationsManager = "OperationsManager";
    public const string Supervisor = "Supervisor";
    public const string Agent = "Agent";
    public const string Viewer = "Viewer";

    public static readonly IReadOnlyList<string> All = [Admin, OperationsManager, Supervisor, Agent, Viewer];
}
