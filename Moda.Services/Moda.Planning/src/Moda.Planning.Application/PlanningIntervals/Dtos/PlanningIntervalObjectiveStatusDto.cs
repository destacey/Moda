namespace Moda.Planning.Application.PlanningIntervals.Dtos;
public sealed record PlanningIntervalObjectiveStatusDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
