using Moda.Planning.Domain.Interfaces;

namespace Moda.Planning.Domain.Models;
public sealed record PlanningIntervalCalendar
{
    private readonly List<ILocalSchedule> _iterationSchedules = new();

    internal PlanningIntervalCalendar(ILocalSchedule planningInterval, IEnumerable<ILocalSchedule> iterations)
    {
        Id = planningInterval.Id;
        Key = planningInterval.Key;
        Name = planningInterval.Name;
        DateRange = planningInterval.DateRange;
        _iterationSchedules = iterations.ToList();
    }

    public Guid Id { get; init; }
    public int Key { get; init; }
    public string Name { get; init; }
    public LocalDateRange DateRange { get; private set; }
    public IReadOnlyList<ILocalSchedule> IterationSchedules
        => _iterationSchedules
            .OrderBy(s => s.DateRange.Start)
                .ThenBy(s => s.DateRange.End)
                    .ThenBy(s => s.Name)
            .ToList()
            .AsReadOnly();

    // Add a new iteration to the calendar
    // Change iteration dates
    // Remove an iteration from the calendar

}
