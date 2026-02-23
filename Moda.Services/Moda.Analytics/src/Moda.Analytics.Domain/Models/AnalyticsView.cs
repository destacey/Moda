using Moda.Analytics.Domain.Enums;
using Moda.Common.Domain.Interfaces;

namespace Moda.Analytics.Domain.Models;

public sealed class AnalyticsView : BaseEntity<Guid>, ISystemAuditable
{
    private AnalyticsView() { }

    private AnalyticsView(
        string name,
        string? description,
        AnalyticsDataset dataset,
        string definitionJson,
        Visibility visibility,
        Guid ownerId,
        bool isActive)
    {
        Name = name;
        Description = description?.Trim();
        Dataset = dataset;
        DefinitionJson = definitionJson;
        Visibility = visibility;
        OwnerId = Guard.Against.NullOrEmpty(ownerId);
        IsActive = isActive;
    }

    public string Name
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    } = default!;

    public string? Description { get; private set; }

    public AnalyticsDataset Dataset { get; private set; }

    public string DefinitionJson
    {
        get;
        private set => field = Guard.Against.NullOrWhiteSpace(value, nameof(DefinitionJson)).Trim();
    } = default!;

    public Visibility Visibility { get; private set; }

    public Guid OwnerId { get; private set; }

    public bool IsActive { get; private set; }

    public Result Update(
        string name,
        string? description,
        AnalyticsDataset dataset,
        string definitionJson,
        Visibility visibility,
        Guid ownerId,
        bool isActive)
    {
        Name = name;
        Description = description?.Trim();
        Dataset = dataset;
        DefinitionJson = definitionJson;
        Visibility = visibility;
        OwnerId = Guard.Against.NullOrEmpty(ownerId);
        IsActive = isActive;

        return Result.Success();
    }

    public static AnalyticsView Create(
        string name,
        string? description,
        AnalyticsDataset dataset,
        string definitionJson,
        Visibility visibility,
        Guid ownerId,
        bool isActive = true)
    {
        return new AnalyticsView(name, description, dataset, definitionJson, visibility, ownerId, isActive);
    }
}
