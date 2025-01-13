using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
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

    /// <summary>
    /// Change the parent Roadmap Activity from the Roadmap Item.
    /// </summary>
    internal Result ChangeParent(RoadmapActivity? newParentActivity)
    {
        if (ParentId == newParentActivity?.Id)
            return Result.Failure("Unable to change the parent because the new parent is the same as the current parent.");

        if (Id == newParentActivity?.Id)
            return Result.Failure("Unable to make the Roadmap Item a child of itself.");

        if (ParentId.HasValue)
        {
            if (Parent is null)
                return Result.Failure("Unable to change the parent because the current parent data has not been loaded.");

            Parent.RemoveChild(Id);
        }

        ParentId = newParentActivity?.Id;
        Parent = newParentActivity;
        newParentActivity?.AddChild(this);

        return Result.Success();
    }
}
