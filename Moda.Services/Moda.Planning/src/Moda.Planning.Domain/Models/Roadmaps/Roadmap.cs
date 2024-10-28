using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Interfaces;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Interfaces;
using Moda.Planning.Domain.Interfaces.Roadmaps;

namespace Moda.Planning.Domain.Models.Roadmaps;
public class Roadmap : BaseAuditableEntity<Guid>, ILocalSchedule, HasIdAndKey
{
    private readonly bool _objectConstruction = false;
    private string _name = default!;
    private string? _description;
    private LocalDateRange _dateRange = default!;
    private readonly List<RoadmapManager> _roadmapManagers = [];
    private readonly List<BaseRoadmapItem> _items = [];

    private Roadmap() { }

    private Roadmap(string name, string? description, LocalDateRange dateRange, Visibility visibility, IEnumerable<Guid> roadmapManagerIds)
    {
        _objectConstruction = true;

        Guard.Against.NullOrEmpty(roadmapManagerIds, nameof(roadmapManagerIds));

        Name = name;
        Description = description;
        DateRange = dateRange;
        Visibility = visibility;

        foreach (var managerId in roadmapManagerIds)
        {
            AddManager(managerId, managerId);
        }

        _objectConstruction = false;
    }

    /// <summary>
    /// The unique key of the Roadmap.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the Roadmap.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// The description of the Roadmap.
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// The date range of the Roadmap.
    /// </summary>
    public LocalDateRange DateRange
    {
        get => _dateRange;
        private set => _dateRange = Guard.Against.Null(value, nameof(DateRange));
    }

    /// <summary>
    /// The visibility of the Roadmap. If the Roadmap is public, all users can see the Roadmap. Otherwise, only the Roadmap Managers can see the Roadmap.
    /// </summary>
    public Visibility Visibility { get; private set; }

    /// <summary>
    /// The managers of the Roadmap. Managers have full control over the Roadmap.
    /// </summary>
    public IReadOnlyList<RoadmapManager> RoadmapManagers => _roadmapManagers.AsReadOnly();

    /// <summary>
    /// The items on the Roadmap.
    /// </summary>
    public IReadOnlyList<BaseRoadmapItem> Items => _items.AsReadOnly();

    private IReadOnlyList<RoadmapActivity> RootActivities => [.. _items.OfType<RoadmapActivity>().Where(x => x.ParentId is null).OrderBy(x => x.Order)];

    /// <summary>
    /// Updates the Roadmap.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="dateRange"></param>
    /// <param name="visibility"></param>
    /// <param name="color"></param>
    /// <param name="currentUserEmployeeId"
    /// <returns></returns>
    public Result Update(string name, string? description, LocalDateRange dateRange, IEnumerable<Guid> roadmapManagerIds, Visibility visibility, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        if (!roadmapManagerIds.Contains(currentUserEmployeeId))
        {
            return Result.Failure("The current user must be a roadmap manager of the Roadmap in order to update it.");
        }

        var syncManagersResult = SyncManagers(roadmapManagerIds, currentUserEmployeeId);
        if (syncManagersResult.IsFailure)
        {
            return syncManagersResult;
        }

        Name = name;
        Description = description;
        DateRange = dateRange;
        Visibility = visibility;

        return Result.Success();
    }

    private Result SyncManagers(IEnumerable<Guid> roadmapManagerIds, Guid currentUserEmployeeId)
    {
        // TODO: This is a temporary solution to sync managers. 
        var managerIdsToAdd = roadmapManagerIds.Where(x => !_roadmapManagers.Any(y => y.ManagerId == x)).ToArray();
        var managerIdsToRemove = _roadmapManagers.Where(x => !roadmapManagerIds.Contains(x.ManagerId)).Select(x => x.ManagerId).ToArray();

        foreach (var managerId in managerIdsToAdd)
        {
            AddManager(managerId, currentUserEmployeeId);
        }

        foreach (var managerId in managerIdsToRemove)
        {
            RemoveManager(managerId, currentUserEmployeeId);
        }

        return Result.Success();
    }

    /// <summary>
    /// Adds a manager to the Roadmap.
    /// </summary>
    /// <param name="roadmapManagerId"></param>
    /// <returns></returns>
    public Result AddManager(Guid roadmapManagerId, Guid currentUserEmployeeId)
    {
        // bypass manager check if no managers exist on initial creation
        if (!_objectConstruction)
        {
            var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
            if (isManagerResult.IsFailure)
            {
                return isManagerResult;
            }
        }

        if (_roadmapManagers.Any(x => x.ManagerId == roadmapManagerId))
        {
            return Result.Failure("Roadmap manager already exists on this roadmap.");
        }

        _roadmapManagers.Add(new RoadmapManager(this, roadmapManagerId));
        return Result.Success();
    }

    /// <summary>
    /// Removes a roadmap manager from the Roadmap.
    /// </summary>
    /// <param name="roadmapManagerId"></param>
    /// <returns></returns>
    public Result RemoveManager(Guid roadmapManagerId, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        var manager = _roadmapManagers.FirstOrDefault(x => x.ManagerId == roadmapManagerId);
        if (manager is null)
        {
            return Result.Failure("Roadmap manager does not exist on this roadmap.");
        }

        if (_roadmapManagers.Count == 1)
        {
            return Result.Failure("Roadmap must have at least one roadmap manager.");
        }

        _roadmapManagers.Remove(manager);
        return Result.Success();
    }


    /// <summary>
    /// Removes a root Roadmap Item from the Roadmap.
    /// </summary>
    /// <param name="childId"></param>
    /// <param name="currentUserEmployeeId"></param>
    /// <returns></returns>
    public Result RemoveChild(Guid childId, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        var child = _items.FirstOrDefault(x => x.Id == childId);
        if (child is null)
        {
            return Result.Failure("Child Roadmap Item does not exist on this roadmap.");
        }

        _items.Remove(child);

        ResetRootActivitiesOrder();

        return Result.Success();
    }

    /// <summary>
    /// Sets the order of the root child Roadmap Items. This is used to set the order of all child roadmap items at once.
    /// </summary>
    /// <param name="childActivities"></param>
    /// <param name="currentUserEmployeeId"></param>
    /// <returns></returns>
    public Result SetChildrenOrder(Dictionary<Guid, int> childActivities, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        if (childActivities.Count != RootActivities.Count)
        {
            return Result.Failure("Not all root roadmap items provided were found.");
        }

        foreach (var child in RootActivities)
        {
            if (!childActivities.ContainsKey(child.Id))
            {
                return Result.Failure("Not all child roadmaps provided were found.");
            }

            var order = childActivities[child.Id];
            if (order < 1)
            {
                return Result.Failure("Order must be greater than 0.");
            }

            child.SetOrder(order);
        }

        ResetRootActivitiesOrder();

        return Result.Success();
    }

    /// <summary>
    /// Updates the order of the root Roadmap Activities based on a single Roadmap Activity changing its order.
    /// </summary>
    /// <param name="childRoadmapId"></param>
    /// <param name="order"></param>
    /// <param name="currentUserEmployeeId"></param>
    /// <returns></returns>
    public Result SetChildrenOrder(Guid childRoadmapId, int order, Guid currentUserEmployeeId)
    {
        if (order < 1)
        {
            return Result.Failure("Order must be greater than 0.");
        }

        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        var rootActivity = RootActivities
            .FirstOrDefault(x => x.Id == childRoadmapId);
        if (rootActivity is null)
        {
            return Result.Failure("Root roadmap activity does not exist on this roadmap.");
        }
        else if (rootActivity.Order == order)
        {
            return Result.Success();
        }

        if (rootActivity.Order < order)
        {
            foreach (var child in RootActivities.Where(x => x.Order > rootActivity.Order && x.Order <= order))
            {
                child.SetOrder(child.Order - 1);
            }
        }
        else
        {
            foreach (var child in RootActivities.Where(x => x.Order >= order && x.Order < rootActivity.Order))
            {
                child.SetOrder(child.Order + 1);
            }
        }

        rootActivity.SetOrder(order);

        ResetRootActivitiesOrder();

        return Result.Success();
    }

    /// <summary>
    /// Resets the order of the root Roadmap Activities. This is used to remove any gaps in the order.
    /// </summary>
    private void ResetRootActivitiesOrder()
    {
        int i = 1;
        foreach (var roadmap in RootActivities)
        {
            roadmap.SetOrder(i);
            i++;
        }
    }

    /// <summary>
    /// Can the Roadmap be deleted.
    /// </summary>
    /// <param name="currentUserEmployeeId"></param>
    /// <returns></returns>
    public Result CanDelete(Guid currentUserEmployeeId)
    {
        return CanEmployeeManage(currentUserEmployeeId);
    }

    /// <summary>
    /// Can the employee manage the Roadmap.
    /// </summary>
    /// <param name="employeeId"></param>
    /// <returns></returns>
    private Result CanEmployeeManage(Guid employeeId)
    {
        if (!_roadmapManagers.Any(x => x.ManagerId == employeeId))
        {
            return Result.Failure("User is not a roadmap manager of this roadmap.");
        }

        return Result.Success();
    }

    /// <summary>
    /// Creates a new Roadmap Activity within the Roadmap.
    /// </summary>
    /// <param name="parentActivityId"></param>
    /// <param name="roadmapActivity"></param>
    /// <param name="currentUserEmployeeId"></param>
    /// <returns></returns>
    public Result<RoadmapActivity> CreateActivity(Guid? parentActivityId, IUpsertRoadmapActivity roadmapActivity, Guid currentUserEmployeeId)
    {
        try
        {
            var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
            if (isManagerResult.IsFailure)
            {
                return Result.Failure<RoadmapActivity>("User is not a roadmap manager of the parent roadmap.");
            }

            RoadmapActivity activity;
            if (parentActivityId.HasValue)
            {
                var parentActivityResult = GetParentRoadmapActivity(parentActivityId.Value);
                if (parentActivityResult.IsFailure)
                {
                    return Result.Failure<RoadmapActivity>(parentActivityResult.Error);
                }

                activity = parentActivityResult.Value.CreateChildActivity(roadmapActivity);
            }
            else
            {
                var order = RootActivities.Count() + 1;
                activity = RoadmapActivity.CreateRoot(Id, roadmapActivity, order);
            }

            _items.Add(activity);

            return activity;
        }
        catch (Exception ex)
        {
            return Result.Failure<RoadmapActivity>(ex.Message);
        }
    }

    //public Result<RoadmapTimebox> CreateRoadmapTimebox(IUpsertRoadmapTimebox timebox, Guid? parentId)
    //{
    //    try
    //    {
    //        RoadmapTimebox newTimebox;

    //        if (parentId.HasValue)
    //        {
    //            var parentActivityResult = GetParentRoadmapActivity(parentId.Value);
    //            if (parentActivityResult.IsFailure)
    //            {
    //                return Result.Failure<RoadmapTimebox>(parentActivityResult.Error);
    //            }

    //            newTimebox = parentActivityResult.Value.CreateChildTimebox(timebox);
    //        }
    //        else
    //        {
    //            newTimebox = RoadmapTimebox.Create(Id, null, timebox);
    //        }

    //        _items.Add(newTimebox);

    //        return newTimebox;
    //    }
    //    catch (Exception ex)
    //    {
    //        return Result.Failure<RoadmapTimebox>(ex.Message);
    //    }
    //}

    //public Result<RoadmapMilestone> CreateRoadmapMilestone(IUpsertRoadmapMilestone milestone, Guid? parentId)
    //{
    //    try
    //    {
    //        RoadmapMilestone newMilestone;

    //        if (parentId.HasValue)
    //        {
    //            var parentActivityResult = GetParentRoadmapActivity(parentId.Value);
    //            if (parentActivityResult.IsFailure)
    //            {
    //                return Result.Failure<RoadmapMilestone>(parentActivityResult.Error);
    //            }

    //            newMilestone = parentActivityResult.Value.CreateChildMilestone(milestone);
    //        }
    //        else
    //        {
    //            newMilestone = RoadmapMilestone.Create(Id, null, milestone);
    //        }

    //        _items.Add(newMilestone);

    //        return newMilestone;
    //    }
    //    catch (Exception ex)
    //    {
    //        return Result.Failure<RoadmapMilestone>(ex.Message);
    //    }
    //}

    public Result UpdateRoadmapActivity(Guid id, IUpsertRoadmapActivity activity, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        var roadmapActivity = _items.OfType<RoadmapActivity>().FirstOrDefault(x => x.Id == id);
        if (roadmapActivity is null)
        {
            return Result.Failure("Roadmap Activity does not exist on this roadmap.");
        }

        var updateResult = roadmapActivity.Update(activity);
        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        if (activity.ParentId == roadmapActivity.ParentId)
        {
            return updateResult;
        }

        // handle changing parent
        RoadmapActivity? newParentActivity = null;
        if (activity.ParentId.HasValue)
        {
            var newParentActivityResult = GetParentRoadmapActivity(activity.ParentId.Value);
            if (newParentActivityResult.IsFailure)
            {
                return Result.Failure(newParentActivityResult.Error);
            }

            newParentActivity = newParentActivityResult.Value;
        }

        var updateRootActivitiesOrder = !roadmapActivity.ParentId.HasValue || newParentActivity is null;
        var changeParentResult = roadmapActivity.ChangeParent(newParentActivity);
        if (changeParentResult.IsFailure)
        {
            return changeParentResult;
        }

        if (updateRootActivitiesOrder)
        {
            ResetRootActivitiesOrder();
        }

        return Result.Success();        
    }

    public Result DeleteItem(Guid itemId, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        var roadmapItem = _items.FirstOrDefault(x => x.Id == itemId);
        if (roadmapItem is null)
        {
            return Result.Failure("Roadmap Item does not exist on this roadmap.");
        }

        var updateRootActivitiesOrder = !roadmapItem.ParentId.HasValue && roadmapItem.Type == RoadmapItemType.Activity;
        if (roadmapItem.ParentId.HasValue)
        {
            var changeParentResult = roadmapItem.ChangeParent(null);
            if (changeParentResult.IsFailure)
            {
                return changeParentResult;
            }
        }

        if (roadmapItem is RoadmapActivity activity)
        {
            var itemIdsToRemove = activity.GetSelfAndDescendants();
            var itemsToRemove = _items.Where(x => itemIdsToRemove.Contains(x.Id)).ToList();
            foreach (var item in itemsToRemove)
            {
                _items.Remove(item);
            }

            if (updateRootActivitiesOrder)
            {
                ResetRootActivitiesOrder();
            }
        }
        else
        {
            _items.Remove(roadmapItem);
        }

        return Result.Success();
    }

    private Result<RoadmapActivity> GetParentRoadmapActivity(Guid parentId)
    {
        var parentActivity = _items.OfType<RoadmapActivity>().FirstOrDefault(x => x.Id == parentId);

        return parentActivity is not null
            ? parentActivity
            : Result.Failure<RoadmapActivity>("Parent Roadmap Activity does not exist on this roadmap.");
    }

    /// <summary>
    /// Creates a new Roadmap.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="dateRange"></param>
    /// <param name="visibility"></param>
    /// <param name="roadmapManagerIds"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static Result<Roadmap> Create(string name, string? description, LocalDateRange dateRange, Visibility visibility, IEnumerable<Guid> roadmapManagerIds)
    {
        try
        {
            return new Roadmap(name, description, dateRange, visibility, roadmapManagerIds);
        }
        catch (Exception ex)
        {
            return Result.Failure<Roadmap>(ex.Message);
        }
    }
}
