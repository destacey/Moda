namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsConnection : Connection<AzureDevOpsBoardsConnectionConfiguration>
{
    private AzureDevOpsBoardsConnection() : base() { }
    private AzureDevOpsBoardsConnection(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid)
    {
        Name = name;
        Description = description;
        Connector = Connector.AzureDevOpsBoards;
        Configuration = configuration;
        IsValidConfiguration = configurationIsValid;
    }

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

    public static AzureDevOpsBoardsConnection Create(string name, string? description, AzureDevOpsBoardsConnectionConfiguration configuration, bool configurationIsValid, Instant timestamp)
    {
        var connector = new AzureDevOpsBoardsConnection(name, description, configuration, configurationIsValid);
        connector.AddDomainEvent(EntityCreatedEvent.WithEntity(connector, timestamp));
        return connector;
    }
}
