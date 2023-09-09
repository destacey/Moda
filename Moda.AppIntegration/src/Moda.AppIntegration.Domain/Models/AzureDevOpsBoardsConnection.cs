namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsConnection : Connection<AzureDevOpsBoardsConnectionConfiguration>
{
    private AzureDevOpsBoardsConnection() : base() { }
    private AzureDevOpsBoardsConnection(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid)
    {
        Name = name;
        Description = description;
        Connector = Connector.AzureDevOpsBoards;
        Configuration = Guard.Against.Null(configuration, nameof(Configuration));
        IsValidConfiguration = configurationIsValid;
    }

    public Result Update(string name, string? description, string organization, string personalAccessToken, bool configurationIsValid, Instant timestamp)
    {
        try
        {
            Guard.Against.Null(Configuration, nameof(Configuration));
            Guard.Against.Null(organization, nameof(organization)); 
            Guard.Against.Null(personalAccessToken, nameof(personalAccessToken));

            Name = name;
            Description = description;
            //Configuration = Configuration! with
            //{
            //    Organization = organization,
            //    PersonalAccessToken = personalAccessToken
            //}; 
            //Configuration.Organization = organization;
            //Configuration.PersonalAccessToken = personalAccessToken;
            IsValidConfiguration = configurationIsValid;

            var newConfiguration = Configuration;
            newConfiguration.Organization = organization;
            newConfiguration.PersonalAccessToken = personalAccessToken;

            Configuration = newConfiguration;

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

            var newConfiguration = Configuration;

            // remove workspaces that are not in the new list
            var workspacesToRemove = newConfiguration.Workspaces.Where(w => !workspaces.Any(nw => nw.Id == w.Id)).ToList();
            foreach (var workspace in workspacesToRemove)
            {
                // TODO - what if the workspace had been configured to sync and has data?
                var result = newConfiguration.RemoveWorkspace(workspace);
                if (result.IsFailure)
                    return result;
            }

            // update existing or add new workspaces
            foreach (var workspace in workspaces)
            {
                var existingWorkspace = newConfiguration.Workspaces.FirstOrDefault(w => w.Id == workspace.Id);
                if (existingWorkspace is not null)
                {
                    var result = existingWorkspace.Update(
                        workspace.Name, 
                        workspace.Description, 
                        existingWorkspace.Sync, 
                        timestamp);

                    if (result.IsFailure)
                        return result;
                }
                else
                {
                    var result = newConfiguration.AddWorkspace(AzureDevOpsBoardsWorkspace.Create(
                        workspace.Id,
                        workspace.Name, 
                        workspace.Description,
                        timestamp));

                    if (result.IsFailure)
                        return result;
                }
            }

            Configuration = newConfiguration;

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
