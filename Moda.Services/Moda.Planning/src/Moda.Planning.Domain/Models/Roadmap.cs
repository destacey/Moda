using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using Moda.Common.Domain.Enums;
using Moda.Planning.Domain.Interfaces;

namespace Moda.Planning.Domain.Models;
public class Roadmap : BaseAuditableEntity<Guid>, ILocalSchedule
{
    private string _name = default!;
    private string? _description;
    private LocalDateRange _dateRange = default!;

    private readonly List<RoadmapManager> _managers = [];

    private Roadmap() { }

    private Roadmap(string name, string? description, LocalDateRange dateRange, Visibility visibility, Guid[] managers)
    {
        Guard.Against.NullOrEmpty(managers, nameof(managers));

        Name = name;
        Description = description;
        DateRange = dateRange;
        Visibility = visibility;

        foreach (var managerId in managers)
        {
            AddManager(managerId);
        }
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
    public IReadOnlyCollection<RoadmapManager> Managers => _managers.AsReadOnly();

    /// <summary>
    /// Updates the Roadmap.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="dateRange"></param>
    /// <param name="visibility"></param>
    /// <param name="currentUserEmployeeId"
    /// <returns></returns>
    public Result Update(string name, string? description, LocalDateRange dateRange, Visibility visibility, Guid currentUserEmployeeId)
    {
        if (!_managers.Any(x => x.ManagerId == currentUserEmployeeId))
        {
            return Result.Failure("User is not a manager of this roadmap.");
        }

        Name = name;
        Description = description;
        DateRange = dateRange;
        Visibility = visibility;

        return Result.Success();
    }

    /// <summary>
    /// Adds a manager to the Roadmap.
    /// </summary>
    /// <param name="managerId"></param>
    /// <returns></returns>
    public Result AddManager(Guid managerId)
    {
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
    public Result RemoveManager(Guid managerId)
    {
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
    /// Creates a new Roadmap.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="dateRange"></param>
    /// <param name="visibility"></param>
    /// <param name="managers"></param>
    /// <returns></returns>
    public static Result<Roadmap> Create(string name, string? description, LocalDateRange dateRange, Visibility visibility, Guid[] managers)
    {
        try
        {
            var roadmap = new Roadmap(name, description, dateRange, visibility, managers);

            return roadmap;
        }
        catch (Exception ex)
        {
            return Result.Failure<Roadmap>(ex.Message);
        }
    }
}
