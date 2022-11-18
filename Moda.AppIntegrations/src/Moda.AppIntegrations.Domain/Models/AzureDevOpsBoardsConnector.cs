namespace Moda.AppIntegrations.Domain.Models;
public sealed class AzureDevOpsBoardsConnector : Connector<AzureDevOpsBoardsConfiguration>
{
    private AzureDevOpsBoardsConnector() : base() { }
    public AzureDevOpsBoardsConnector(string name, string? description, AzureDevOpsBoardsConfiguration configuration) 
        : base(name, description, ConnectorType.AzureDevOpsBoards, configuration)
    {
    }

    public Result Update(string name, string? description, AzureDevOpsBoardsConfiguration? configuration)
    {
        Name = name;
        Description = description;
        Configuration = configuration;

        return Result.Success();
    }

    public override void ValidateConfiguration()
    {
        IsValidConfiguration = Configuration is not null && Configuration.IsValid();
    }
}
