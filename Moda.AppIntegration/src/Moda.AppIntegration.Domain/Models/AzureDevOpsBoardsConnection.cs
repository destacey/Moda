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

    public Result Update(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid, Instant timestamp)
    {
        try
        {
            Name = name;
            Description = description;
            Configuration = Guard.Against.Null(configuration, nameof(Configuration));
            IsValidConfiguration = configurationIsValid;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result UpdateConfiguration(AzureDevOpsBoardsConnectionConfiguration configuration, Instant timestamp)
    {
        try
        {
            Configuration = Guard.Against.Null(configuration, nameof(Configuration));

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
            var workspacesToRemove = Configuration.Workspaces.Where(w => !workspaces.Any(nw => nw.Id == w.Id)).ToList();
            foreach (var workspace in workspacesToRemove)
            {
                var result = Configuration.RemoveWorkspace(workspace);
                if (result.IsFailure)
                    return result;
            }

            // update existing or add new workspaces
            foreach (var workspace in workspaces)
            {
                var existingWorkspace = Configuration.Workspaces.FirstOrDefault(w => w.Id == workspace.Id);
                if (existingWorkspace is not null)
                {
                    var result = existingWorkspace.Update(
                        workspace.Name, 
                        workspace.Description, 
                        existingWorkspace.Import, 
                        timestamp);

                    if (result.IsFailure)
                        return result;
                }
                else
                {
                    var result = Configuration.AddWorkspace(AzureDevOpsBoardsWorkspace.Create(
                        workspace.Id,
                        workspace.Name, 
                        workspace.Description,
                        timestamp));

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
