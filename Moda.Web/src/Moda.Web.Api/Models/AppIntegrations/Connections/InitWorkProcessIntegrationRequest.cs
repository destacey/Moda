namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record InitWorkProcessIntegrationRequest
{
    /// <summary>
    /// Connection Id.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// External identifier for the work process.
    /// </summary>
    public Guid ExternalId { get; set; }
}

public sealed class InitWorkProcessIntegrationRequestValidator : CustomValidator<InitWorkProcessIntegrationRequest>
{
    public InitWorkProcessIntegrationRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Id)
            .NotEmpty();

        RuleFor(t => t.ExternalId)
            .NotEmpty();
    }
}
