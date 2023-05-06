namespace Moda.Web.Api.Models.Identity.Roles;

public sealed record UpdateRolePermissionsRequest
{
    public string RoleId { get; set; } = null!;
    public List<string> Permissions { get; set; } = null!;

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
