namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record AzdoConnectionTeamMappingRequest
{
    /// <summary>
    /// The unique identifer for the connection.
    /// </summary>
    public Guid ConnectionId { get; set; }

    /// <summary>
    /// List of team mappings.
    /// </summary>
    public List<AzdoWorkspaceTeamMappingRequest> TeamMappings { get; set; } = [];
}

public sealed class AzdoConnectionTeamMappingRequestValidator : CustomValidator<AzdoConnectionTeamMappingRequest>
{
    public AzdoConnectionTeamMappingRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.ConnectionId)
            .NotEmpty();
    }
}
