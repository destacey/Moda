using Moda.Common.Extensions;
using Moda.Planning.Application.EstimationScales.Dtos;
using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.PokerSessions.Dtos;

public class PokerSessionDetailsDto : IMapFrom<PokerSession>
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required string Status { get; set; }
    public Guid FacilitatorId { get; set; }
    public string FacilitatorName { get; set; } = default!;
    public EstimationScaleDetailsDto? EstimationScale { get; set; }
    public Instant? ActivatedOn { get; set; }
    public Instant? CompletedOn { get; set; }
    public List<PokerRoundDto> Rounds { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<PokerSession, PokerSessionDetailsDto>()
            .Map(dest => dest.Status, src => src.Status.GetDisplayName())
            .Map(dest => dest.FacilitatorName, src => src.Facilitator == null ? string.Empty : src.Facilitator.Name.FullName);
    }
}
