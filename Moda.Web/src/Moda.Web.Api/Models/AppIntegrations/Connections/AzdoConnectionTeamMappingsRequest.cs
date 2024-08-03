namespace Moda.Web.Api.Models.AppIntegrations.Connections;

public sealed record AzdoConnectionTeamMappingsRequest
{
    /// <summary>
    /// The unique identifer for the connection.
    /// </summary>
    public Guid ConnectionId { get; set; }

    /// <summary>
    /// List of team mappings.
    /// </summary>
    public List<AzdoWorkspaceTeamMappingRequest> TeamMappings { get; set; } = [];

    public UpdateAzureDevOpsConnectionTeamMappingsCommand ToUpdateAzureDevOpsConnectionTeamMappingsCommand()
    {
        var teamMappings = TeamMappings
            .Select(t => t.ToAzureDevOpsWorkspaceTeamMappingDto())
            .ToList();
        return new UpdateAzureDevOpsConnectionTeamMappingsCommand(ConnectionId, teamMappings);
    }
}

public sealed class AzdoConnectionTeamMappingsRequestValidator : CustomValidator<AzdoConnectionTeamMappingsRequest>
{
    public AzdoConnectionTeamMappingsRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.ConnectionId)
            .NotEmpty();
    }
}
