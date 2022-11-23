namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsConnector : Connector<AzureDevOpsBoardsConnectorConfiguration>
{
    private AzureDevOpsBoardsConnector() : base() { }
    private AzureDevOpsBoardsConnector(string name, string? description)
    {
        Name = name;
        Description = description;
        Type = ConnectorType.AzureDevOpsBoards;
    }

    public Result Update(string name, string? description, Instant timestamp)
    {
        try
        {
            Name = name;
            Description = description;

            ValidateConfiguration();
            
            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public Result UpdateConfiguration(AzureDevOpsBoardsConnectorConfiguration? configuration, Instant timestamp)
    {
        try
        {
            Configuration = configuration;

            ValidateConfiguration();

            AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public override void ValidateConfiguration()
    {
        IsValidConfiguration = Configuration?.IsValid() ?? false;
    }

    public static AzureDevOpsBoardsConnector Create(string name, string? description, Instant timestamp)
    {
        var connector = new AzureDevOpsBoardsConnector(name, description);
        connector.AddDomainEvent(EntityCreatedEvent.WithEntity(connector, timestamp));
        return connector;
    }
}
