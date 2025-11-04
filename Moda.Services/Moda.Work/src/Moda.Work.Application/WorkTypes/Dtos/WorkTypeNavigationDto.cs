using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.WorkTypes.Dtos;

public sealed record WorkTypeNavigationDto : IMapFrom<WorkType>
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the work type.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The work type level name.
    /// </summary>
    public required SimpleNavigationDto Level { get; set; }

    /// <summary>
    /// The work type tier.
    /// </summary>
    public required SimpleNavigationDto Tier { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkType, WorkTypeNavigationDto>()
            .Map(dest => dest.Level, src => SimpleNavigationDto.Create(src.LevelId, src.Level!.Name))
            .Map(dest => dest.Tier, src => SimpleNavigationDto.FromEnum(src.Level!.Tier));
    }
}
