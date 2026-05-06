namespace Wayd.Web.Api.Models.UserManagement.Users;

public sealed record StageTenantMigrationRequest(string TargetTenantId);

public sealed class StageTenantMigrationRequestValidator : CustomValidator<StageTenantMigrationRequest>
{
    public StageTenantMigrationRequestValidator()
    {
        // The Application-layer StageTenantMigrationCommandValidator covers the same
        // rules, but it only fires through the MediatR pipeline. The controller calls
        // IUserService directly, so we duplicate the rules here to get 422 responses
        // with field-level errors before reaching the service.
        RuleFor(r => r.TargetTenantId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _))
                .WithMessage("Target tenant id must be a valid GUID.");
    }
}
