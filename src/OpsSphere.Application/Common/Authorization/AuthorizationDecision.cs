namespace OpsSphere.Application.Common.Authorization;

public sealed record AuthorizationDecision(bool IsAllowed, string? ReasonCode = null)
{
    public static AuthorizationDecision Allowed() => new(true);
    public static AuthorizationDecision Denied(string reasonCode) => new(false, reasonCode);

    // Safe reason codes - do not add resource details here
    public static class ReasonCodes
    {
        public const string Unauthenticated = "unauthenticated";
        public const string InactiveUser = "inactive_user";
        public const string RoleMissing = "role_missing";
        public const string PermissionMissing = "permission_missing";
        public const string ScopeMissing = "scope_missing";
        public const string ScopeDenied = "scope_denied";
        public const string ResourceNotFound = "resource_not_found";
        public const string UnsupportedScopeType = "unsupported_scope_type";
    }
}
