using Moda.Common.Application.Dtos;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Dtos;
public sealed record RoadmapActivityDto : RoadmapItemDto, IMapFrom<RoadmapActivity>
{
    /// <summary>
    /// The Roadmap Activity start date.
    /// </summary>
    public required LocalDate Start { get; set; }

    /// <summary>
    /// The Roadmap Activity end date.
    /// </summary>
    public required LocalDate End { get; set; }

    /// <summary>
    /// The order of the Roadmap Activity within its parent.
    /// </summary>
    public int Order { get; set; }

    public override void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<RoadmapActivity, RoadmapActivityDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End)
            .Map(dest => dest.Type, src => SimpleNavigationDto.FromEnum(src.Type));
    }
}
