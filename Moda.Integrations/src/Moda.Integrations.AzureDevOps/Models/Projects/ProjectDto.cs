using Moda.Integrations.AzureDevOps.Models.Contracts;

namespace Moda.Integrations.AzureDevOps.Models.Projects;
internal record ProjectDto
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
}

internal static class ProjectDtoExtensions
{
    public static AzdoWorkspace ToAzdoWorkspace(this ProjectDto project)
    {
        return new AzdoWorkspace
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
        };
    }

    public static List<AzdoWorkspace> ToAzdoWorkspaces(this List<ProjectDto> projects)
    {
        return projects.Select(p => p.ToAzdoWorkspace()).ToList();
    }
}
