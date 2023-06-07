namespace Moda.Planning.Application.ProgramIncrements.Dtos;
public sealed record ProgramIncrementObjectiveTypeDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int Order { get; set; }
}
