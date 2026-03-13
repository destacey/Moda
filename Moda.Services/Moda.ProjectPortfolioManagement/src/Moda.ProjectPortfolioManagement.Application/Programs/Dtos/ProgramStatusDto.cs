using System.ComponentModel.DataAnnotations;

namespace Moda.ProjectPortfolioManagement.Application.Programs.Dtos;

public sealed record ProgramStatusDto
{
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public int Order { get; set; }

    [Required]
    public required string LifecyclePhase { get; set; }
}
