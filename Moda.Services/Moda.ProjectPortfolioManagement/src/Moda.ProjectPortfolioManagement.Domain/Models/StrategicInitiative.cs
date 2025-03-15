using System.Data;
using Ardalis.GuardClauses;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

public sealed class StrategicInitiative : BaseEntity<Guid>, ISystemAuditable, IHasIdAndKey
{
    private string _name = default!;
    private string? _description;
    private LocalDateRange _dateRange = default!;

    private readonly HashSet<RoleAssignment<StrategicInitiativeRole>> _roles = [];
    private readonly HashSet<StrategicInitiativeKpi> _kpis = [];
    private readonly HashSet<Project> _projects = [];

    private StrategicInitiative() { }

    private StrategicInitiative(string name, string? description, StrategicInitiativeStatus status, LocalDateRange dateRange, Guid portfolioId, Dictionary<StrategicInitiativeRole, HashSet<Guid>>? roles = null)
    {
        Name = name;
        Description = description;
        Status = status;
        DateRange = dateRange;
        PortfolioId = portfolioId;

        _roles = roles?
            .SelectMany(r => r.Value
                .Select(e => new RoleAssignment<StrategicInitiativeRole>(Id, r.Key, e)))
            .ToHashSet()
            ?? [];
    }

    /// <summary>
    /// The unique key of the strategic initiative.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; private init; }

    /// <summary>
    /// The name of the strategic initiative.
    /// </summary>
    public string Name
    {
        get => _name;
        private set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>
    /// A detailed explanation of what the strategic initiative aims to achieve.
    /// </summary>
    public string? Description
    {
        get => _description;
        private set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>
    /// The status of the strategic initiative.
    /// </summary>
    public StrategicInitiativeStatus Status { get; private set; }

    /// <summary>
    /// The date range for the strategic initiative.
    /// </summary>
    public LocalDateRange DateRange
    {
        get => _dateRange;
        private set => _dateRange = Guard.Against.Null(value, nameof(DateRange));
    }

    /// <summary>
    /// The Id of the portfolio to which this strategic initiative belongs.
    /// </summary>
    public Guid PortfolioId { get; private set; }

    /// <summary>
    /// The portfolio to which this strategic initiative belongs.
    /// </summary>
    public ProjectPortfolio? Portfolio { get; private set; }

    /// <summary>
    /// The roles associated with the strategic initiative.
    /// </summary>
    public IReadOnlyCollection<RoleAssignment<StrategicInitiativeRole>> Roles => _roles;

    /// <summary>
    /// The KPIs associated with this strategic initiative.
    /// </summary>
    public IReadOnlyCollection<StrategicInitiativeKpi> Kpis => _kpis;

    /// <summary>
    /// The projects associated with this strategic initiative.
    /// </summary>
    public IReadOnlyCollection<Project> Projects => _projects;

    /// <summary>
    /// Indicates if the strategic initiative is in a closed state.
    /// </summary>
    public bool IsClosed => Status is StrategicInitiativeStatus.Completed or StrategicInitiativeStatus.Cancelled;

    /// Indicates whether the strategic initiative can be deleted.
    /// </summary>
    /// <returns></returns>
    public bool CanBeDeleted() => Status is StrategicInitiativeStatus.Proposed or StrategicInitiativeStatus.Approved;

    public static StrategicInitiative Create(string name, string? description, StrategicInitiativeStatus status, LocalDateRange dateRange, Guid portfolioId, Dictionary<StrategicInitiativeRole, HashSet<Guid>>? roles = null)
    {
        return new StrategicInitiative(name, description, status, dateRange, portfolioId, roles);
    }
}
