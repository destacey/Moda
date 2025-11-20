using Moda.Common.Domain.Interfaces.ProjectPortfolioManagement;
using Moda.Common.Models;
using NodaTime;

namespace Moda.Common.Domain.Events.ProjectPortfolioManagement;
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

    public Guid Id { get; init; }
    public int Key { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }
    public int StatusId { get; set; }
    public LocalDateRange? DateRange { get; set; }
    public Guid PortfolioId { get; set; }

    /// <summary>
    /// The roles for the program.  The key is the role type id and the value is an array of user ids.
    /// </summary>
    public Dictionary<int, Guid[]>? Roles { get; set; }

    /// <summary>
    /// The strategic theme ids for the program.
    /// </summary>
    public Guid[] StrategicThemes { get; set; }
}