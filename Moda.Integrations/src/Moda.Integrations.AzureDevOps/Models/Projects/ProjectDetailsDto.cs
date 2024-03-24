using Ardalis.GuardClauses;
using Moda.Integrations.AzureDevOps.Models.Contracts;

namespace Moda.Integrations.AzureDevOps.Models.Projects;
internal sealed record ProjectDetailsDto : ProjectDto
{
    public List<PropertyDto> Properties { get; set; } = [];

    public bool HasProcessTemplateType => Properties.Any(p => p.Name == "System.ProcessTemplateType");

    public static ProjectDetailsDto Create(ProjectDto projectDto, List<PropertyDto> propertyDtos)
    {
        Guard.Against.Null(projectDto, nameof(projectDto));
        Guard.Against.Null(propertyDtos, nameof(propertyDtos));

        return new ProjectDetailsDto
        {

            Id = projectDto.Id,
            Name = projectDto.Name,
            Description = projectDto.Description,
            Properties = propertyDtos
        };
    }
}

internal static class ProjectDetailsDtoExtensions
{
    public static AzdoWorkspaceConfiguration ToAzdoWorkspaceConfiguration(this ProjectDetailsDto project)
    {
        var processTemplateType = project.Properties.SingleOrDefault(p => p.Name == "System.ProcessTemplateType")?.Value;
        Guard.Against.Null(processTemplateType, "System.ProcessTemplateType");

        return new AzdoWorkspaceConfiguration
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            WorkProcessId = new Guid(processTemplateType)
        };
    }
}
