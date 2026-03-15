using Moda.Common.Application.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Projects.Dtos;

public sealed record ProjectPhaseListDto : IMapFrom<ProjectPhase>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required SimpleNavigationDto Status { get; set; }
    public int Order { get; set; }
    public LocalDate? Start { get; set; }
    public LocalDate? End { get; set; }
    public decimal Progress { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectPhase, ProjectPhaseListDto>()
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Start, src => src.DateRange != null ? src.DateRange.Start : (LocalDate?)null)
            .Map(dest => dest.End, src => src.DateRange != null ? src.DateRange.End : (LocalDate?)null)
            .Map(dest => dest.Progress, src => src.Progress.Value);
    }
}
