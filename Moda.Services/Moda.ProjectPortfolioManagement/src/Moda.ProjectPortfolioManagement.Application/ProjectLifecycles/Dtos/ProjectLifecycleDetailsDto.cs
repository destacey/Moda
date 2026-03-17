using Moda.Common.Application.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.ProjectLifecycles.Dtos;

public sealed record ProjectLifecycleDetailsDto : IMapFrom<ProjectLifecycle>
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required SimpleNavigationDto State { get; set; }
    public required List<ProjectLifecyclePhaseDto> Phases { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<ProjectLifecycle, ProjectLifecycleDetailsDto>()
            .Map(dest => dest.State, src => SimpleNavigationDto.FromEnum(src.State));
    }
}
