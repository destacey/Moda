namespace Moda.Common.Application.Identity.Roles;

public sealed record UpdateRolePermissionsCommand
{
    public UpdateRolePermissionsCommand(string roleId, List<string> permissions)
    {
        RoleId = roleId;
        Permissions = permissions;
    }

    public string RoleId { get; }
    public List<string> Permissions { get; } = new();
}

public sealed class UpdateRolePermissionsCommandValidator : CustomValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(r => r.RoleId)
            .NotEmpty();
        RuleFor(r => r.Permissions)
            .NotNull();
    }
}