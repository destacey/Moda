using Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;
using Moda.Common.Domain.Models.ProjectPortfolioManagement;
using Moda.Common.Models;
using NodaTime;

namespace Moda.Common.Domain.Events.ProjectPortfolioManagement;
public sealed record ProjectCreatedEvent : DomainEvent, ISimpleProject
{
    public ProjectCreatedEvent(ISimpleProject project, int expenditureCategoryId, int statusId, LocalDateRange? dateRange, Guid portfolioId, Guid? programId, Dictionary<int, Guid[]> roles, Guid[] strategicThemes, Instant timestamp)
    {
        Id = project.Id;
        Key = project.Key;
        Name = project.Name;
        Description = project.Description;
        ExpenditureCategoryId = expenditureCategoryId;
        StatusId = statusId;
        DateRange = dateRange;
        PortfolioId = portfolioId;
        ProgramId = programId;
        Roles = roles.ToDictionary(x => x.Key, x => x.Value.ToArray());
        StrategicThemes = [.. strategicThemes];

        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public ProjectKey Key { get; }
    public string Name { get; }
    public string Description { get; }
    public int ExpenditureCategoryId { get; }
    public int StatusId { get; }
    public LocalDateRange? DateRange { get; }
    public Guid PortfolioId { get; }
    public Guid? ProgramId { get; }

    /// <summary>
    /// The roles for the project.  The key is the role type id and the value is an array of user ids.
    /// </summary>
    public Dictionary<int, Guid[]>? Roles { get; }

    /// <summary>
    /// The strategic theme ids for the project.
    /// </summary>
    public Guid[] StrategicThemes { get; }
}