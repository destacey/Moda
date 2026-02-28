using Moda.Common.Application.Dtos;
using Moda.Common.Extensions;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.PokerSessions.Dtos;

public class PokerRoundDto : IMapFrom<PokerRound>
{
    public Guid Id { get; set; }
    public string Label { get; set; } = default!;
    public required string Status { get; set; }
    public string? ConsensusEstimate { get; set; }
    public int Order { get; set; }
    public int VoteCount { get; set; }
    public List<VoteDto> Votes { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<PokerRound, PokerRoundDto>()
            .Map(dest => dest.Status, src => src.Status.GetDisplayName())
            .Map(dest => dest.VoteCount, src => src.Votes.Count)
            .Map(dest => dest.Votes, src => src.Status >= PokerRoundStatus.Revealed ? src.Votes : new List<PokerVote>());
    }
}
