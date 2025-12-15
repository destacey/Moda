using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;

/// <summary>
/// Navigation DTO for a project task.
/// </summary>
public sealed record ProjectTaskNavigationDto : IMapFrom<ProjectTask>
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string TaskKey { get; set; }
    public required string Name { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectTask, ProjectTaskNavigationDto>()
            .Map(dest => dest.TaskKey, src => src.TaskKey.Value);
    }

    public static ProjectTaskNavigationDto From(ProjectTask task)
    {
        return new ProjectTaskNavigationDto
        {
            Id = task.Id,
            Key = task.Key,
            TaskKey = task.TaskKey.Value,
            Name = task.Name
        };
    }
}
