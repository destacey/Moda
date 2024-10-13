﻿using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Roadmaps.Dtos;
public sealed record RoadmapMilestoneDto : RoadmapItemDto, IMapFrom<RoadmapMilestone>
{
    /// <summary>
    /// The Roadmap Milestone date.
    /// </summary>
    public required LocalDate Date { get; set; }

    public override void ConfigureMapping(TypeAdapterConfig config)
    {
        base.ConfigureMapping(config);
        config.NewConfig<RoadmapMilestone, RoadmapMilestoneDto>()
            .Map(dest => dest.Date, src => src.Date);
    }
}
