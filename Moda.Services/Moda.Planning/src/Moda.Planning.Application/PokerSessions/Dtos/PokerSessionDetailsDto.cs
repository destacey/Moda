using Moda.Common.Application.Identity.Users;
using Moda.Common.Extensions;
using Moda.Planning.Application.EstimationScales.Dtos;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.PokerSessions.Dtos;

public sealed record PokerSessionDetailsDto : IMapFrom<PokerSession>
{
    private static readonly HashSet<string> _revealedStatuses =
    [
        PokerRoundStatus.Revealed.GetDisplayName(),
        PokerRoundStatus.Accepted.GetDisplayName()
    ];

    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required string Status { get; set; }
    public UserNavigationDto? Facilitator { get; set; }
    public EstimationScaleDto? EstimationScale { get; set; }
    public Instant? ActivatedOn { get; set; }
    public Instant? CompletedOn { get; set; }
    public List<PokerRoundDto> Rounds { get; set; } = [];

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<PokerSession, PokerSessionDetailsDto>()
            .Map(dest => dest.Status, src => src.Status.GetDisplayName())
            .Map(dest => dest.Rounds, src => src.Rounds.OrderBy(r => r.Order));
    }

    /// <summary>
    /// Hides vote values for rounds that have not yet been revealed.
    /// Participant identity is preserved so the UI can show who has voted.
    /// </summary>
    public void HideUnrevealedVotes()
    {
        foreach (var round in Rounds)
        {
            if (!_revealedStatuses.Contains(round.Status))
            {
                foreach (var vote in round.Votes)
                {
                    vote.Value = string.Empty;
                }
            }
        }
    }
}
