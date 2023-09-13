namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsConnection : Connection<AzureDevOpsBoardsConnectionConfiguration>
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private AzureDevOpsBoardsConnection() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private AzureDevOpsBoardsConnection(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid)
    {
        Name = name;
        Description = description;
        Connector = Connector.AzureDevOpsBoards;
        Configuration = Guard.Against.Null(configuration, nameof(Configuration));
        IsValidConfiguration = configurationIsValid;
    }

    public override AzureDevOpsBoardsConnectionConfiguration Configuration { get; protected set; }

    public Result Update(string name, string? description, string organization, string personalAccessToken, bool configurationIsValid, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));
            Guard.Against.Null(organization, nameof(organization)); 
            Guard.Against.Null(personalAccessToken, nameof(personalAccessToken));

            Name = name;
            Description = description;
            IsValidConfiguration = configurationIsValid;

            Configuration.Organization = organization;
            Configuration.PersonalAccessToken = personalAccessToken;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result ImportWorkspaces(IEnumerable<AzureDevOpsBoardsWorkspace> workspaces, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));

            // remove workspaces that are not in the new list
            var workspacesToRemove = Configuration.Workspaces.Where(w => !workspaces.Any(nw => nw.ExternalId == w.ExternalId)).ToList();
            foreach (var workspace in workspacesToRemove)
            {
                // TODO - what if the workspace had been configured to sync and has data?
                var result = Configuration.RemoveWorkspace(workspace);
                if (result.IsFailure)
                    return result;
            }

            // update existing or add new workspaces
            foreach (var workspace in workspaces)
            {
                var existingWorkspace = Configuration.Workspaces.FirstOrDefault(w => w.ExternalId == workspace.ExternalId);
                if (existingWorkspace is not null)
                {
                    var result = existingWorkspace.Update(
                        workspace.Name, 
                        workspace.Description, 
                        existingWorkspace.Sync);

                    if (result.IsFailure)
                        return result;
                }
                else
                {
                    var result = Configuration.AddWorkspace(AzureDevOpsBoardsWorkspace.Create(
                        workspace.ExternalId,
                        workspace.Name, 
                        workspace.Description));

                    if (result.IsFailure)
                        return result;
                }
            }

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsConnection Create(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid, Instant timestamp)
    {
        var connector = new AzureDevOpsBoardsConnection(name, description, configuration, configurationIsValid);
        connector.AddDomainEvent(EntityCreatedEvent.WithEntity(connector, timestamp));
        return connector;
    }
}
