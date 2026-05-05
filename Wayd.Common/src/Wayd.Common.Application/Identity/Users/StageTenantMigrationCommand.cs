namespace Wayd.Common.Application.Identity.Users;

public sealed record StageTenantMigrationCommand(string UserId, string TargetTenantId);

public sealed class StageTenantMigrationCommandValidator : CustomValidator<StageTenantMigrationCommand>
{
    public StageTenantMigrationCommandValidator()
    {
        RuleFor(c => c.UserId)
            .NotEmpty();

        // Entra tenant ids are always GUIDs. Auth0 tenants aren't, but this command
        // is Entra-only today — gate strictly so a typo'd tid doesn't sit on a user
        // forever waiting for a login that can never match.
        RuleFor(c => c.TargetTenantId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
                .WithMessage("Target tenant id must be a valid GUID.");
    }
}
