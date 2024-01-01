using Moda.Planning.Domain.Interfaces;

namespace Moda.Planning.Application.PlanningIntervals.Dtos;
public sealed record LocalScheduleDto
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required LocalDate Start { get; set; }
    public required LocalDate End { get; set; }

    public static LocalScheduleDto Create(ILocalSchedule schedule)
    {
        return new LocalScheduleDto
        {
            Id = schedule.Id,
            Key = schedule.Key,
            Name = schedule.Name,
            Start = schedule.DateRange.Start,
            End = schedule.DateRange.End
        };
    }
}
