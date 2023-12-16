namespace Moda.Planning.Application.PlanningIntervals.Dtos;
public sealed record PlanningIntervalObjectiveTypeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Order { get; set; }
}
