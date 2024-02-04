using Moda.Common.Domain.Models;
using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsWorkProcess : IntegrationObject<Guid>
{
    private string _name = null!;
    private string? _description;

    private AzureDevOpsBoardsWorkProcess() { }
    private AzureDevOpsBoardsWorkProcess(Guid externalId, string name, string? description, IntegrationState<Guid>? integrationState)
    {
        ExternalId = externalId;
        Name = name;
        Description = description;
        IntegrationState = integrationState;
    }

    public Guid ExternalId { get; private init; }

    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    public Result Update(string name, string? description)
    {
        try
        {
            Name = name;
            Description = description;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsWorkProcess Create(Guid externalId, string name, string? description)
    {
        return new AzureDevOpsBoardsWorkProcess(externalId, name, description, null);
    }
}
