using FluentValidation;

namespace Moda.Core.Application.Identity.Roles;

public class CreateOrUpdateRoleRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateOrUpdateRoleRequestValidator : CustomValidator<CreateOrUpdateRoleRequest>
{
    public CreateOrUpdateRoleRequestValidator(IRoleService roleService) =>
        RuleFor(r => r.Name)
            .NotEmpty()
            .MustAsync(async (role, name, _) => !await roleService.ExistsAsync(name, role.Id))
                .WithMessage("Similar Role already exists.");
}