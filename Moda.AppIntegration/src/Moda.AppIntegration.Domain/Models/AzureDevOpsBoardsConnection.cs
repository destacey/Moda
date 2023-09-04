namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsConnection : Connection<AzureDevOpsBoardsConnectionConfiguration>
{
    private readonly List<AzureDevOpsBoardsWorkspaceConfiguration> _workspaces = new();

    private AzureDevOpsBoardsConnection() : base() { }
    private AzureDevOpsBoardsConnection(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid)
    {
        Name = name;
        Description = description;
        Connector = Connector.AzureDevOpsBoards;
        Configuration = configuration;
        IsValidConfiguration = configurationIsValid;
    }

    public IReadOnlyCollection<AzureDevOpsBoardsWorkspaceConfiguration> Workspaces => _workspaces.AsReadOnly();

    public Result Update(string name, string? description, AzureDevOpsBoardsConnectionConfiguration? configuration, bool configurationIsValid, Instant timestamp)
    {
        try
        {
            Name = name;
            Description = description;
            Configuration = configuration;
            IsValidConfiguration = configurationIsValid;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result UpdateConfiguration(AzureDevOpsBoardsConnectionConfiguration? configuration, Instant timestamp)
    {
        try
        {
            Configuration = configuration;

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result ImportWorkspaces(IEnumerable<AzureDevOpsBoardsWorkspaceConfiguration> workspaces, Instant timestamp)
    {
        try
        {
            // remove workspaces that are not in the new list
            var workspacesToRemove = _workspaces.Where(w => !workspaces.Any(nw => nw.Id == w.Id)).ToList();
            foreach (var workspace in workspacesToRemove)
            {
                _workspaces.Remove(workspace);
            }

            // update existing or add new workspaces
            foreach (var workspace in workspaces)
            {
                var existingWorkspace = _workspaces.FirstOrDefault(w => w.Id == workspace.Id);
                if (existingWorkspace is not null)
                {
                    existingWorkspace.Update(
                        workspace.Name, 
                        workspace.Description, 
                        existingWorkspace.Import, 
                        timestamp);
                }
                else
                {
                    _workspaces.Add(AzureDevOpsBoardsWorkspaceConfiguration.Create(
                        workspace.Id,
                        workspace.Name, 
                        workspace.Description,
                        Id,
                        timestamp));
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
