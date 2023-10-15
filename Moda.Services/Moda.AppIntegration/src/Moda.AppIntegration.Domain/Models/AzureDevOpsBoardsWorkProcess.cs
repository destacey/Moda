using Moda.Common.Extensions;

namespace Moda.AppIntegration.Domain.Models;
public sealed class AzureDevOpsBoardsWorkProcess
{
    private string _name = null!;
    private string? _description;

    private AzureDevOpsBoardsWorkProcess() { }
    private AzureDevOpsBoardsWorkProcess(Guid externalId, string name, string? description)
    {
        ExternalId = externalId;
        Name = name;
        Description = description;
    }

    public Guid ExternalId { get; init; }

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

    public static AzureDevOpsBoardsWorkProcess Create(Guid id, string name, string? description)
    {
        return new AzureDevOpsBoardsWorkProcess(id, name, description);
    }
}
