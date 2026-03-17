using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Dtos;

public sealed record ProjectPhaseDetailsDto : IMapFrom<ProjectPhase>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required SimpleNavigationDto Status { get; set; }
    public int Order { get; set; }
    public LocalDate? Start { get; set; }
    public LocalDate? End { get; set; }
    public decimal Progress { get; set; }
    public List<EmployeeNavigationDto> Assignees { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectPhase, ProjectPhaseDetailsDto>()
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Start, src => src.DateRange != null ? src.DateRange.Start : (LocalDate?)null)
            .Map(dest => dest.End, src => src.DateRange != null ? src.DateRange.End : (LocalDate?)null)
            .Map(dest => dest.Progress, src => src.Progress.Value)
            .Map(dest => dest.Assignees, src => src.Roles
                .Where(r => r.Role == ProjectPhaseRole.Assignee)
                .Select(r => EmployeeNavigationDto.From(r.Employee!)));
    }
}
