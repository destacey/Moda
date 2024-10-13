using Mapster;
using Moda.Common.Application.Dtos;
using Moda.Planning.Application.Roadmaps.Dtos;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application;
public class MapsterRoadmapItemConfig
{
    //public static void Configure()
    //{
    //    // This is set here instead of the IMapFrom interface to make sure it is applied correctly
    //    TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

    //    TypeAdapterConfig<BaseRoadmapItem, RoadmapItemDto>.NewConfig()
    //        .Include<RoadmapActivity, RoadmapActivityDto>()
    //        .Include<RoadmapMilestone, RoadmapMilestoneDto>()
    //        .Include<RoadmapTimebox, RoadmapTimeboxDto>()
    //        .Map(dest => dest.Id, src => src.Id)
    //        .Map(dest => dest.RoadmapId, src => src.RoadmapId)
    //        .Map(dest => dest.Name, src => src.Name)
    //        .Map(dest => dest.Type, src => SimpleNavigationDto.FromEnum(src.Type))
    //        .Map(dest => dest.Parent, src => src.Parent)
    //        .Map(dest => dest.Color, src => src.Color)
    //        .AfterMapping((src, dest) =>
    //        {
    //            dest = src.Type switch
    //            {
    //                RoadmapItemType.Activity => src.Adapt<RoadmapActivityDto>(),
    //                RoadmapItemType.Milestone => src.Adapt<RoadmapMilestoneDto>(),
    //                RoadmapItemType.Timebox => src.Adapt<RoadmapTimeboxDto>(),
    //                _ => dest
    //            };
    //        });

    //    TypeAdapterConfig<RoadmapActivity, RoadmapActivityDto>.NewConfig()
    //        .IgnoreNullValues(true)
    //        .Map(dest => dest.Start, src => src.DateRange.Start)
    //        .Map(dest => dest.End, src => src.DateRange.End)
    //        .Map(dest => dest.Order, src => src.Order);

    //    TypeAdapterConfig<RoadmapMilestone, RoadmapMilestoneDto>.NewConfig()
    //        .IgnoreNullValues(true)
    //        .Map(dest => dest.Date, src => src.Date);

    //    TypeAdapterConfig<RoadmapTimebox, RoadmapTimeboxDto>.NewConfig()
    //        .IgnoreNullValues(true)
    //        .Map(dest => dest.Start, src => src.DateRange.Start)
    //        .Map(dest => dest.End, src => src.DateRange.End);
    //}
}
