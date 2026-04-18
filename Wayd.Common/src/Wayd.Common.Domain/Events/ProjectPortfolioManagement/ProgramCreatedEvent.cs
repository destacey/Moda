using Wayd.Common.Domain.Interfaces.ProjectPortfolioManagement;
using Wayd.Common.Models;
using NodaTime;

namespace Wayd.Common.Domain.Events.ProjectPortfolioManagement;

public sealed record ProgramCreatedEvent : DomainEvent, ISimpleProgram
{
    public ProgramCreatedEvent(ISimpleProgram project, int statusId, LocalDateRange? dateRange, Guid portfolioId, Dictionary<int, Guid[]> roles, Guid[] strategicThemes, Instant timestamp)
    {
        Id = project.Id;
        Key = project.Key;
        Name = project.Name;
        Description = project.Description;
        StatusId = statusId;
        DateRange = dateRange;
        PortfolioId = portfolioId;
        Roles = roles.ToDictionary(x => x.Key, x => x.Value.ToArray());
        StrategicThemes = [.. strategicThemes];

        Timestamp = timestamp;
    }

    public Guid Id { get; }
    public int Key { get; }
    public string Name { get; }
    public string Description { get; }
    public int StatusId { get; }
    public LocalDateRange? DateRange { get; }
    public Guid PortfolioId { get; }

    /// <summary>
    /// The roles for the program.  The key is the role type id and the value is an array of user ids.
    /// </summary>
    public Dictionary<int, Guid[]>? Roles { get; }

    /// <summary>
    /// The strategic theme ids for the program.
    /// </summary>
    public Guid[] StrategicThemes { get; }
}