using Moda.Common.Domain.Models;
using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsWorkspace : IntegrationObject<Guid>
{
    private string _name = null!;
    private string? _description;

    private AzureDevOpsBoardsWorkspace() { }
    private AzureDevOpsBoardsWorkspace(Guid externalId, string name, string? description, Guid? workProcessId, IntegrationState<Guid>? integrationState)
    {
        ExternalId = externalId;
        Name = name;
        Description = description;
        WorkProcessId = workProcessId;
        IntegrationState = integrationState;
    }

    public Guid ExternalId { get; private init; }

    /// <summary>Gets or sets the name of the connection.</summary>
    /// <value>The name of the connection.</value>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>Gets or sets the description.</summary>
    /// <value>The connection description.</value>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets or sets the work process identifier.</summary>
    public Guid? WorkProcessId { get; private set; }

    public Result Update(string name, string? description, Guid? workProcessId)
    {
        try
        {
            Name = name;
            Description = description;
            WorkProcessId = workProcessId;

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    public static AzureDevOpsBoardsWorkspace Create(Guid externalId, string name, string? description, Guid? workProcessId)
    {
        return new AzureDevOpsBoardsWorkspace(externalId, name, description, workProcessId, null);
    }
}
