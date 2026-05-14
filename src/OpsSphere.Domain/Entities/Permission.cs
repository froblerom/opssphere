namespace OpsSphere.Domain.Entities;

public sealed class Permission
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
