using Ardalis.GuardClauses;
using Moda.Common.Domain.Interfaces;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Domain.Models.Roadmaps;
public abstract class BaseRoadmapItem : BaseEntity<Guid>, ISystemAuditable
{
    private string _name = default!;
    private string? _description;
    private string? _color;

    /// <summary>
    /// The Roadmap Id.
    /// </summary>
    public Guid RoadmapId { get; protected init; }

    /// <summary>
    /// The name of the Roadmap.
    /// </summary>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the Roadmap.
    /// </summary>
    public string? Description
    {
        get => _description;
        protected set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// The type of the Roadmap Item.
    /// </summary>
    public RoadmapItemType Type { get; protected init; }

    /// <summary>
    /// The parent Roadmap Activity Id. This is used to connect Roadmap Items together.
    /// </summary>
    public Guid? ParentId { get; protected set; }

    /// <summary>
    /// The parent Roadmap Activity. This is used to connect Roadmap Items together.
    /// </summary>
    public RoadmapActivity? Parent { get; protected set; }

    /// <summary>
    /// The color of the Roadmap. This is used to display the Roadmap in the UI.
    /// </summary>
    public string? Color { get => _color; protected set => _color = value.NullIfWhiteSpacePlusTrim(); }
}
