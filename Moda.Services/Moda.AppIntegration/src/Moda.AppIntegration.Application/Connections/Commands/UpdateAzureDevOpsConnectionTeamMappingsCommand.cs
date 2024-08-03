using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Moda.AppIntegration.Application.Connections.Commands;
public sealed record UpdateAzureDevOpsConnectionTeamMappingsCommand(Guid ConnectionId, List<AzureDevOpsWorkspaceTeamMappingDto> TeamMappings) : ICommand;

public sealed class AzdoConnectionTeamMappingsRequestValidator : CustomValidator<UpdateAzureDevOpsConnectionTeamMappingsCommand>
{
    public AzdoConnectionTeamMappingsRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.ConnectionId)
            .NotEmpty();

        RuleForEach(t => t.TeamMappings)
            .NotNull()
            .SetValidator(new AzureDevOpsWorkspaceTeamMappingDtoValidator());
    }
}

internal sealed class UpdateAzureDevOpsConnectionTeamMappingsCommandHandler : ICommandHandler<UpdateAzureDevOpsConnectionTeamMappingsCommand>
{
    private const string AppRequestName = nameof(UpdateAzureDevOpsConnectionTeamMappingsCommand);

    private readonly IAppIntegrationDbContext _appIntegrationDbContext;
    private readonly ILogger<UpdateAzureDevOpsConnectionTeamMappingsCommandHandler> _logger;

    public UpdateAzureDevOpsConnectionTeamMappingsCommandHandler(IAppIntegrationDbContext appIntegrationDbContext, ILogger<UpdateAzureDevOpsConnectionTeamMappingsCommandHandler> logger)
    {
        _appIntegrationDbContext = appIntegrationDbContext;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateAzureDevOpsConnectionTeamMappingsCommand request, CancellationToken cancellationToken)
    {
        var connection = await _appIntegrationDbContext.AzureDevOpsBoardsConnections
            .FirstOrDefaultAsync(c => c.Id == request.ConnectionId, cancellationToken);
        if (connection is null)
            return Result.Failure("Azure DevOps Boards connection not found.");

        var teams = connection.TeamConfiguration.WorkspaceTeams;
        foreach (var team in request.TeamMappings)
        {
            var existingTeam = teams.SingleOrDefault(w => w.WorkspaceId == team.WorkspaceId && w.TeamId == team.ExternalTeamId);
            if (existingTeam is not null)
            {
                if (existingTeam.InternalTeamId != team.InternalTeamId)
                {
                    existingTeam.MapInternalTeam(team.InternalTeamId);
                    _logger.LogDebug("{AppRequestName}: Team mapping updated for connection {ConnectionId} workspace {WorkspaceId} and team {ExternalTeamId}.", AppRequestName, request.ConnectionId, team.WorkspaceId, team.ExternalTeamId);
                }
            }
            else
            {
                _logger.LogWarning("{AppRequestName}: Team mapping not found for connection {ConnectionId} workspace {WorkspaceId} and team {ExternalTeamId}.", AppRequestName, request.ConnectionId, team.WorkspaceId, team.ExternalTeamId);
            }
        }

        await _appIntegrationDbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{AppRequestName}: Connection {ConnectionId} team mappings updated.", AppRequestName, request.ConnectionId);

        return Result.Success();
    }
}

