namespace Moda.Web.Api.Models.UserManagement.Roles;

public sealed record UpdateRolePermissionsRequest
{
    public string RoleId { get; set; } = default!;
    public List<string> Permissions { get; set; } = default!;

    public UpdateRolePermissionsCommand ToUpdateRolePermissionsCommand()
        => new(RoleId, Permissions);
}

public sealed class UpdateRolePermissionsRequestValidator : CustomValidator<UpdateRolePermissionsRequest>
{
    public UpdateRolePermissionsRequestValidator()
    {
        RuleFor(r => r.RoleId)
            .NotEmpty();
        RuleFor(r => r.Permissions)
            .NotNull();
    }
}
