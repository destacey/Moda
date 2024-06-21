using Moda.Common.Domain.Enums.Work;
using Moda.Work.Domain.Interfaces;

namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record WorkItemParentInfo : IMapFrom<WorkItem>, IWorkItemParentInfo
{
    public Guid Id { get; init; }
    public int? ExternalId { get; init; }
    public WorkTypeTier Tier { get; init; }
    public int LevelOrder { get; init; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItem, WorkItemParentInfo>()
            .Map(dest => dest.LevelOrder, src => src.Type.Level!.Order)
            .Map(dest => dest.Tier, src => src.Type.Level!.Tier);
    }
}
