﻿using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Dtos;
public sealed record RoadmapActivityDetailsDto : RoadmapItemDetailsDto, IMapFrom<RoadmapActivity>
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

    public List<RoadmapItemDetailsDto> Children { get; set; } = [];

    public override void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<RoadmapActivity, RoadmapActivityDetailsDto>()
            .Inherits<BaseRoadmapItem, RoadmapItemDetailsDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End);
    }
}
