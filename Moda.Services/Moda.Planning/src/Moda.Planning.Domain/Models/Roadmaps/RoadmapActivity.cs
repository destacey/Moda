using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Domain.Models.Roadmaps;

/// <summary>
/// An activity is a core component of the roadmap, representing a theme, project, piece of work or a group of related tasks.
/// </summary>
public sealed class RoadmapActivity : BaseRoadmapItem
{
    private LocalDateRange _dateRange = default!;
    private readonly List<BaseRoadmapItem> _children = [];

    private RoadmapActivity() { }

    internal RoadmapActivity(Guid roadmapId, string name, string? description, LocalDateRange dateRange, Guid? parentId, string? color, int order)
    {
        RoadmapId = roadmapId;
        Name = name;
        Description = description;
        Type = RoadmapItemType.Activity;
        DateRange = dateRange;
        ParentId = parentId;
        Color = color;
        Order = order;
    }

    /// <summary>
    /// The date range of the Roadmap Activity.
    /// </summary>
    public LocalDateRange DateRange
    {
        get => _dateRange;
        private set => _dateRange = Guard.Against.Null(value, nameof(DateRange));
    }

    /// <summary>
    /// The order of the Activity within the parent Roadmap or Roadmap Activity.
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// The children of the Roadmap Activity.
    /// </summary>
    public IReadOnlyList<BaseRoadmapItem> Children => _children.AsReadOnly();

    /// <summary>
    /// Updates the Roadmap Activity.
    /// </summary>
    /// <param name="roadmapActivity"></param>
    /// <returns></returns>
    internal Result Update(IUpsertRoadmapActivity roadmapActivity, RoadmapActivity? parentActivity)
    {
        // TODO: this initial implementation requires going through the Roadmap to update the Roadmap Activity. This is needed to verify permissions against the Roadmap within the Domain layer.

        if (ParentId != roadmapActivity.ParentId)
        {
            var changeParentResult = ChangeParent(parentActivity);
            if (changeParentResult.IsFailure)
            {
                return changeParentResult;
            }
        }

        Name = roadmapActivity.Name;
        Description = roadmapActivity.Description;
        DateRange = roadmapActivity.DateRange;
        Color = roadmapActivity.Color;

        return Result.Success();
    }

    /// <summary>
    /// Updates the date range of the Roadmap Activity.
    /// </summary>
    /// <param name="dateRange"></param>
    /// <returns></returns>
    internal Result UpdateDateRange(IUpsertRoadmapActivityDateRange dateRange)
    {
        DateRange = dateRange.DateRange;
        return Result.Success();
    }

    /// <summary>
    /// Sets the order of the Roadmap Activity.
    /// </summary>
    /// <param name="order"></param>
    internal void SetOrder(int order)
    {
        Order = order;
    }

    /// <summary>
    /// Sets the order of a child Roadmap Activity and resets the order of the other child Roadmap Activities to match.
    /// </summary>
    /// <param name="activity"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    internal Result SetChildActivityOrder(RoadmapActivity activity, int order)
    {
        // TODO: merge this with the SetActivityOrder on Roadmap.

        if (!_children.OfType<RoadmapActivity>().Any(x => x.Id == activity.Id))
        {
            return Result.Failure("Child activity not found.");
        }
        else if (activity.Order == order)
        {
            return Result.Success();
        }

        if (activity.Order < order)
        {
            foreach (var child in _children.OfType<RoadmapActivity>()
                .Where(x => x.Order > activity.Order && x.Order <= order))
            {
                child.SetOrder(child.Order - 1);
            }
        }
        else
        {
            foreach (var child in _children.OfType<RoadmapActivity>()
                .Where(x => x.Order >= order && x.Order < activity.Order))
            {
                child.SetOrder(child.Order + 1);
            }
        }

        activity.SetOrder(order);

        ResetChildActivitiesOrder();

        return Result.Success();
    }

    /// <summary>
    /// Resets the order of the child Roadmap Activities. This is used to remove any gaps in the order.
    /// </summary>
    internal void ResetChildActivitiesOrder()
    {
        int i = 1;
        foreach (var activity in _children.OfType<RoadmapActivity>().OrderBy(x => x.Order).ToArray())
        {
            activity.SetOrder(i);
            i++;
        }
    }

    /// <summary>
    /// Creates a new Roadmap Activity within an existing Roadmap Activity.
    /// </summary>
    /// <param name="roadmapId"></param>
    /// <param name="roadmapActivity"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    internal RoadmapActivity CreateChildActivity(IUpsertRoadmapActivity roadmapActivity)
    {
        var order = Children.Count + 1;

        var newRoadmapActivity = new RoadmapActivity(RoadmapId, roadmapActivity.Name, roadmapActivity.Description, roadmapActivity.DateRange, Id, roadmapActivity.Color, order);

        _children.Add(newRoadmapActivity);

        return newRoadmapActivity;
    }

    /// <summary>
    /// Creates a new Roadmap Timebox within an existing Roadmap Activity.
    /// </summary>
    /// <param name="timebox"></param>
    /// <returns></returns>
    internal RoadmapTimebox CreateChildTimebox(IUpsertRoadmapTimebox timebox)
    {
        var newTimebox = RoadmapTimebox.Create(RoadmapId, Id, timebox);

        _children.Add(newTimebox);

        return newTimebox;
    }

    /// <summary>
    /// Creates a new Roadmap Milestone within an existing Roadmap Activity.
    /// </summary>
    /// <param name="milestone"></param>
    /// <returns></returns>
    internal RoadmapMilestone CreateChildMilestone(IUpsertRoadmapMilestone milestone)
    {
        var newMilestone = RoadmapMilestone.Create(RoadmapId, Id, milestone);

        _children.Add(newMilestone);

        return newMilestone;
    }

    /// <summary>
    /// Adds an existing child Roadmap Item to the Roadmap Activity.
    /// </summary>
    /// <param name="child"></param>
    /// <returns></returns>
    internal Result AddChild(BaseRoadmapItem child)
    {
        if (_children.Any(x => x.Id == child.Id))
        {
            return Result.Failure("Child already exists under this Roadmap Activity.");
        }

        switch (child)
        {
            case RoadmapActivity roadmapActivity:
                _children.Add(roadmapActivity);
                roadmapActivity.SetOrder(_children.Count);
                break;
            case RoadmapTimebox roadmapTimebox:
                _children.Add(roadmapTimebox);
                break;
            case RoadmapMilestone roadmapMilestone:
                _children.Add(roadmapMilestone);
                break;
            default:
                return Result.Failure("Child type not supported.");
        }

        return Result.Success();
    }

    /// <summary>
    /// Removes a child Roadmap Item from the Roadmap Activity.
    /// </summary>
    /// <param name="roadmapId"></param>
    /// <returns></returns>
    internal Result RemoveChild(Guid roadmapId)
    {
        var child = _children.FirstOrDefault(x => x.Id == roadmapId);

        if (child == null)
        {
            return Result.Failure("Child not found.");
        }

        _children.Remove(child);

        ResetChildActivitiesOrder();

        return Result.Success();
    }

    /// <summary>
    /// Gets the Roadmap Activity and all of its descendants.
    /// </summary>
    /// <returns></returns>
    internal List<Guid> GetSelfAndDescendants()
    {
        var ids = new List<Guid> { Id };

        foreach (var child in _children)
        {
            if (child is RoadmapActivity roadmapActivity)
            {
                ids.AddRange(roadmapActivity.GetSelfAndDescendants());
            }
            else
            {
                ids.Add(child.Id);
            }
        }

        return ids;
    }

    /// <summary>
    /// Creates a new Roadmap Activity at the root of the Roadmap.
    /// </summary>
    /// <param name="roadmapId"></param>
    /// <param name="activity"></param>
    /// <param name="order"></param>
    /// <returns></returns>
    internal static RoadmapActivity CreateRoot(Guid roadmapId, IUpsertRoadmapActivity activity, int order)
    {
        // TODO: this initial implementation requires going through the Roadmap to create the Roadmap Activity. This is needed to verify permissions against the Roadmap within the Domain layer.

        return new RoadmapActivity(roadmapId, activity.Name, activity.Description, activity.DateRange, null, activity.Color, order);

    }
}
