using Moda.Analytics.Domain.Enums;
using Moda.Common.Domain.Interfaces;

namespace Moda.Analytics.Domain.Models;

public sealed class AnalyticsView : BaseEntity<Guid>, ISystemAuditable
{
    private bool _objectConstruction;
    private readonly List<AnalyticsViewManager> _analyticsViewManagers = [];

    private AnalyticsView() { }

    private AnalyticsView(
        string name,
        string? description,
        AnalyticsDataset dataset,
        string definitionJson,
        Visibility visibility,
        IEnumerable<Guid> managerIds,
        bool isActive)
    {
        _objectConstruction = true;

        Guard.Against.NullOrEmpty(managerIds, nameof(managerIds));

        Name = name;
        Description = description?.Trim();
        Dataset = dataset;
        DefinitionJson = definitionJson;
        Visibility = visibility;
        IsActive = isActive;

        foreach (var managerId in managerIds)
        {
            AddManager(managerId, managerId);
        }

        _objectConstruction = false;
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

    public bool IsActive { get; private set; }

    public IReadOnlyList<AnalyticsViewManager> AnalyticsViewManagers => _analyticsViewManagers.AsReadOnly();

    public Result Update(
        string name,
        string? description,
        AnalyticsDataset dataset,
        string definitionJson,
        Visibility visibility,
        IEnumerable<Guid> managerIds,
        bool isActive,
        Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        if (!managerIds.Contains(currentUserEmployeeId))
        {
            return Result.Failure("The current user must be a manager of the Analytics View in order to update it.");
        }

        var syncManagersResult = SyncManagers(managerIds, currentUserEmployeeId);
        if (syncManagersResult.IsFailure)
        {
            return syncManagersResult;
        }

        Name = name;
        Description = description?.Trim();
        Dataset = dataset;
        DefinitionJson = definitionJson;
        Visibility = visibility;
        IsActive = isActive;

        return Result.Success();
    }

    public static Result<AnalyticsView> Create(
        string name,
        string? description,
        AnalyticsDataset dataset,
        string definitionJson,
        Visibility visibility,
        IEnumerable<Guid> managerIds,
        bool isActive = true)
    {
        try
        {
            return new AnalyticsView(name, description, dataset, definitionJson, visibility, managerIds, isActive);
        }
        catch (Exception ex)
        {
            return Result.Failure<AnalyticsView>(ex.Message);
        }
    }

    public Result CanEmployeeManage(Guid employeeId)
    {
        return _analyticsViewManagers.Any(x => x.ManagerId == employeeId)
            ? Result.Success()
            : Result.Failure("User is not a manager of this analytics view.");
    }

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

        if (_analyticsViewManagers.Any(x => x.ManagerId == managerId))
        {
            return Result.Failure("Manager already exists on this analytics view.");
        }

        _analyticsViewManagers.Add(new AnalyticsViewManager(this, managerId));
        return Result.Success();
    }

    public Result RemoveManager(Guid managerId, Guid currentUserEmployeeId)
    {
        var isManagerResult = CanEmployeeManage(currentUserEmployeeId);
        if (isManagerResult.IsFailure)
        {
            return isManagerResult;
        }

        var manager = _analyticsViewManagers.FirstOrDefault(x => x.ManagerId == managerId);
        if (manager is null)
        {
            return Result.Failure("Manager does not exist on this analytics view.");
        }

        if (_analyticsViewManagers.Count == 1)
        {
            return Result.Failure("Analytics view must have at least one manager.");
        }

        _analyticsViewManagers.Remove(manager);
        return Result.Success();
    }

    private Result SyncManagers(IEnumerable<Guid> managerIds, Guid currentUserEmployeeId)
    {
        var managerIdsToAdd = managerIds.Where(x => !_analyticsViewManagers.Any(y => y.ManagerId == x)).ToArray();
        var managerIdsToRemove = _analyticsViewManagers.Where(x => !managerIds.Contains(x.ManagerId)).Select(x => x.ManagerId).ToArray();

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
}
