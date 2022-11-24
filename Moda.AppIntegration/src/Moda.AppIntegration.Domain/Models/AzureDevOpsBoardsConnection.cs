namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsConnection : Connection<AzureDevOpsBoardsConnectionConfiguration>
{
    private AzureDevOpsBoardsConnection() : base() { }
    private AzureDevOpsBoardsConnection(string name, string? description)
    {
        Name = name;
        Description = description;
        Connector = Connector.AzureDevOpsBoards;
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

    public Result UpdateConfiguration(AzureDevOpsBoardsConnectionConfiguration? configuration, Instant timestamp)
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

    public static AzureDevOpsBoardsConnection Create(string name, string? description, Instant timestamp)
    {
        var connector = new AzureDevOpsBoardsConnection(name, description);
        connector.AddDomainEvent(EntityCreatedEvent.WithEntity(connector, timestamp));
        return connector;
    }
}
