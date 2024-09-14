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

    private readonly List<RoadmapManager> _managers = [];
    private readonly List<Roadmap> _children = [];

    private Roadmap() { }

    private Roadmap(string name, string? description, LocalDateRange dateRange, Visibility visibility, Guid[] managers, Guid? parentId, int? orderId)
    {
        _objectConstruction = true;

        Guard.Against.NullOrEmpty(managers, nameof(managers));

        Name = name;
        Description = description;
        DateRange = dateRange;
        Visibility = visibility;

        ParentId = parentId;
        Order = orderId;

        foreach (var managerId in managers)
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
    public IReadOnlyList<RoadmapManager> Managers => _managers.AsReadOnly();

    /// <summary>
    /// Updates the Roadmap.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="dateRange"></param>
    /// <param name="visibility"></param>
    /// <param name="currentUserEmployeeId"
    /// <returns></returns>
    public Result Update(string name, string? description, LocalDateRange dateRange, IEnumerable<Guid> managerIds, Visibility visibility, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        if (!managerIds.Contains(currentUserEmployeeId))
        {
            return Result.Failure("The current user must be a manager of the Roadmap in order to update it.");
        }

        var syncManagersResult = SyncManagers(managerIds, currentUserEmployeeId);
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

    private Result SyncManagers(IEnumerable<Guid> managerIds, Guid currentUserEmployeeId)
    {
        // TODO: This is a temporary solution to sync managers. 
        var managerIdsToAdd = managerIds.Where(x => !_managers.Any(y => y.ManagerId == x)).ToArray();
        var managerIdsToRemove = _managers.Where(x => !managerIds.Contains(x.ManagerId)).Select(x => x.ManagerId).ToArray();

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
    /// <param name="managerId"></param>
    /// <returns></returns>
    public Result AddManager(Guid managerId, Guid currentUserEmployeeId)
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

        if (_managers.Any(x => x.ManagerId == managerId))
        {
            return Result.Failure("Roadmap manager already exists on this roadmap.");
        }

        _managers.Add(new RoadmapManager(this, managerId));
        return Result.Success();
    }

    /// <summary>
    /// Removes a manager from the Roadmap.
    /// </summary>
    /// <param name="managerId"></param>
    /// <returns></returns>
    public Result RemoveManager(Guid managerId, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        var manager = _managers.FirstOrDefault(x => x.ManagerId == managerId);
        if (manager is null)
        {
            return Result.Failure("Roadmap manager does not exist on this roadmap.");
        }

        if (_managers.Count == 1)
        {
            return Result.Failure("Roadmap must have at least one manager.");
        }

        _managers.Remove(manager);
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
        if (!_managers.Any(x => x.ManagerId == employeeId))
        {
            return Result.Failure("User is not a manager of this roadmap.");
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
    /// <param name="managers"></param>
    /// <returns></returns>
    public Result<Roadmap> CreateChild(string name, string? description, LocalDateRange dateRange, Visibility visibility, Guid[] managers, Guid currentUserEmployeeId)
    {
        try
        {
            var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
            if (isManagerResult.IsFailure)
            {
                return Result.Failure<Roadmap>(isManagerResult.Error);
            }

            var order = _children.Count + 1;

            var roadmap = new Roadmap(name, description, dateRange, visibility, managers, Id, order);

            _children.Add(roadmap);

            return roadmap;
        }
        catch (Exception ex)
        {
            return Result.Failure<Roadmap>(ex.Message);
        }
    }

    /// <summary>
    /// Creates a new Roadmap as a child of an existing Roadmap.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="dateRange"></param>
    /// <param name="visibility"></param>
    /// <param name="managers"></param>
    /// <param name="parentId"></param>
    /// <param name="orderId"></param>
    /// <returns></returns>
    //public static Result<Roadmap> CreateChild(string name, string? description, LocalDateRange dateRange, Visibility visibility, Guid[] managers, Guid parentId, int orderId)
    //{
    //    try
    //    {
    //        var roadmap = new Roadmap(name, description, dateRange, visibility, managers, parentId, orderId);

    //        return roadmap;
    //    }
    //    catch (Exception ex)
    //    {
    //        return Result.Failure<Roadmap>(ex.Message);
    //    }
    //}

    /// <summary>
    /// Creates a new Roadmap.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="dateRange"></param>
    /// <param name="visibility"></param>
    /// <param name="managers"></param>
    /// <returns></returns>
    public static Result<Roadmap> CreateRoot(string name, string? description, LocalDateRange dateRange, Visibility visibility, Guid[] managers)
    {
        try
        {
            var roadmap = new Roadmap(name, description, dateRange, visibility, managers, null, null);

            return roadmap;
        }
        catch (Exception ex)
        {
            return Result.Failure<Roadmap>(ex.Message);
        }
    }
}
