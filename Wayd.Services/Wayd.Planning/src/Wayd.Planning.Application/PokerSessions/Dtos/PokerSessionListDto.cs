using Wayd.Common.Application.Identity.Users;
using Wayd.Common.Extensions;
using Wayd.Planning.Domain.Models.PlanningPoker;

namespace Wayd.Planning.Application.PokerSessions.Dtos;

public sealed record PokerSessionListDto : IMapFrom<PokerSession>
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required string Status { get; set; }
    public UserNavigationDto? Facilitator { get; set; }
    public int RoundCount { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<PokerSession, PokerSessionListDto>()
            .Map(dest => dest.Status, src => src.Status.GetDisplayName())
            .Map(dest => dest.RoundCount, src => src.Rounds.Count);
    }
}
