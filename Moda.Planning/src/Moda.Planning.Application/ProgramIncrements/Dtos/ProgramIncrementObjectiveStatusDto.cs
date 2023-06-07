namespace Moda.Planning.Application.ProgramIncrements.Dtos;
public sealed record ProgramIncrementObjectiveStatusDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
