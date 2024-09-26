using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums;
using Moda.Common.Domain.Interfaces;
using Moda.Planning.Domain.Interfaces;

namespace Moda.Planning.Domain.Models;
public class Roadmap : BaseAuditableEntity<Guid>, ILocalSchedule, HasIdAndKey
{
    private readonly bool _objectConstruction = false;
    private string _name = default!;
    private string? _description;
    private LocalDateRange _dateRange = default!;
    private string? _color;
    private readonly List<RoadmapManager> _roadmapManagers = [];
    private readonly List<Roadmap> _children = [];

    private Roadmap() { }

    private Roadmap(string name, string? description, LocalDateRange dateRange, Visibility visibility, IEnumerable<Guid> roadmapManagerIds, Guid? parentId, int? orderId, string? color)
    {
        _objectConstruction = true;

        Guard.Against.NullOrEmpty(roadmapManagerIds, nameof(roadmapManagerIds));

        Name = name;
        Description = description;
        DateRange = dateRange;
        Visibility = visibility;
        Color = color;

        ParentId = parentId;
        Order = orderId;

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
    /// The color of the Roadmap. This is used to display the Roadmap in the UI.
    /// </summary>
    public string? Color { get => _color; private set => _color = value.NullIfWhiteSpacePlusTrim(); }

    /// <summary>
    /// The parent Roadmap Id. This is used to connect Roadmaps together.
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>
    /// The parent Roadmap. This is used to connect Roadmaps together.
    /// </summary>
    public Roadmap? Parent { get; private set; }

    /// <summary>
    /// The order of the Roadmap within the parent Roadmap.
    /// </summary>
    public int? Order { get; private set; }

    /// <summary>
    /// The children of the Roadmap. Children are used to connect Roadmaps together.
    /// </summary>
    public IReadOnlyList<Roadmap> Children => _children.AsReadOnly();

    /// <summary>
    /// The managers of the Roadmap. Managers have full control over the Roadmap.
    /// </summary>
    public IReadOnlyList<RoadmapManager> RoadmapManagers => _roadmapManagers.AsReadOnly();

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
    public Result Update(string name, string? description, LocalDateRange dateRange, IEnumerable<Guid> roadmapManagerIds, Visibility visibility, string? color, Guid currentUserEmployeeId)
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
        Color = color;

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
    /// Removes a child Roadmap from the Roadmap.
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

        var child = _children.FirstOrDefault(x => x.Id == childId);
        if (child is null)
        {
            return Result.Failure("Child Roadmap does not exist on this roadmap.");
        }

        _children.Remove(child);

        child.ParentId = null;
        child.Order = null;

        ResetChildrenOrder();

        return Result.Success();
    }

    /// <summary>
    /// Sets the order of the child Roadmaps. This is used to set the order of all child roadmaps at once.
    /// </summary>
    /// <param name="childRoadmaps"></param>
    /// <param name="currentUserEmployeeId"></param>
    /// <returns></returns>
    public Result SetChildrenOrder(Dictionary<Guid, int> childRoadmaps, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        if (childRoadmaps.Count != _children.Count)
        {
            return Result.Failure("Not all child roadmaps provided were found.");
        }

        foreach (var child in _children)
        {
            if (!childRoadmaps.ContainsKey(child.Id))
            {
                return Result.Failure("Not all child roadmaps provided were found.");
            }

            var order = childRoadmaps[child.Id];
            if (order < 1)
            {
                return Result.Failure("Order must be greater than 0.");
            }

            child.Order = order;
        }

        ResetChildrenOrder();

        return Result.Success();
    }

    /// <summary>
    /// Updates the order of the child Roadmap based on a single roadmap changing its order.
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

        var childRoadmap = _children.FirstOrDefault(x => x.Id == childRoadmapId);
        if (childRoadmap is null)
        {
            return Result.Failure("Child roadmap does not exist on this roadmap.");
        }

        if (childRoadmap.Order == order)
        {
            return Result.Success();
        }

        if (childRoadmap.Order < order)
        {
            foreach (var child in _children.Where(x => x.Order > childRoadmap.Order && x.Order <= order))
            {
                child.Order = child.Order - 1;
            }
        }
        else
        {
            foreach (var child in _children.Where(x => x.Order >= order && x.Order < childRoadmap.Order))
            {
                child.Order = child.Order + 1;
            }
        }

        childRoadmap.Order = order;

        ResetChildrenOrder();

        return Result.Success();
    }

    /// <summary>
    /// Resets the order of the child Roadmap links. This is used to remove any gaps in the order.
    /// </summary>
    private void ResetChildrenOrder()
    {
        int i = 1;
        foreach (var roadmap in _children.OrderBy(x => x.Order))
        {
            roadmap.Order = i;
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
    /// Creates a new Roadmap as a child of this Roadmap.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="dateRange"></param>
    /// <param name="visibility"></param>
    /// <param name="roadmapManagerIds"></param>
    /// <param name="color"></param>
    /// <param name="currentUserEmployeeId"></param>
    /// <returns></returns>
    public Result<Roadmap> CreateChild(string name, string? description, LocalDateRange dateRange, Visibility visibility, IEnumerable<Guid> roadmapManagerIds, string? color, Guid currentUserEmployeeId)
    {
        try
        {
            var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
            if (isManagerResult.IsFailure)
            {
                return Result.Failure<Roadmap>("User is not a roadmap manager of the parent roadmap.");
            }

            var order = _children.Count + 1;

            var roadmap = new Roadmap(name, description, dateRange, visibility, roadmapManagerIds, Id, order, color);

            _children.Add(roadmap);

            return roadmap;
        }
        catch (Exception ex)
        {
            return Result.Failure<Roadmap>(ex.Message);
        }
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
    public static Result<Roadmap> CreateRoot(string name, string? description, LocalDateRange dateRange, Visibility visibility, IEnumerable<Guid> roadmapManagerIds, string? color)
    {
        try
        {
            var roadmap = new Roadmap(name, description, dateRange, visibility, roadmapManagerIds, null, null, color);

            return roadmap;
        }
        catch (Exception ex)
        {
            return Result.Failure<Roadmap>(ex.Message);
        }
    }
}
