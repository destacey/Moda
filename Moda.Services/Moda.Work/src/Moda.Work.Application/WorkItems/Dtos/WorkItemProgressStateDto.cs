using Moda.Common.Domain.Enums.Work;

namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record WorkItemProgressStateDto : IMapFrom<WorkItem>
{
    public Guid Id { get; set; }
    public int LevelOrder { get; set; }
    public WorkTypeTier Tier { get; set; }
    public WorkStatusCategory StatusCategory { get; set; }
    public Guid? ParentId { get; set; }
    public Instant Created { get; set; }
    public Instant? ActivatedTimestamp { get; set; }
    public Instant? DoneTimestamp { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkItem, WorkItemProgressStateDto>()
            .Map(dest => dest.LevelOrder, src => src.Type.Level!.Order)
            .Map(dest => dest.Tier, src => src.Type.Level!.Tier);
    }
}
