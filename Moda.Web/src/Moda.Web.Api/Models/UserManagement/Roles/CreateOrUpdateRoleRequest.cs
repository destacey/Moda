namespace Moda.Web.Api.Models.UserManagement.Roles;

public sealed record CreateOrUpdateRoleRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public CreateOrUpdateRoleCommand ToCreateOrUpdateRoleCommand()
        => new(Id, Name, Description);
}

public sealed class CreateOrUpdateRoleRequestValidator : CustomValidator<CreateOrUpdateRoleRequest>
{
    public CreateOrUpdateRoleRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(r => r.Name)
            .NotEmpty()
            .MaximumLength(256);
    }
}
