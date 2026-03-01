using Moda.Common.Application.Employees.Dtos;
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
    public EmployeeNavigationDto? Facilitator { get; set; }
    public EstimationScaleDetailsDto? EstimationScale { get; set; }
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
    /// Hides votes for rounds that have not yet been revealed.
    /// </summary>
    public void HideUnrevealedVotes()
    {
        foreach (var round in Rounds)
        {
            if (!_revealedStatuses.Contains(round.Status))
            {
                round.Votes = [];
            }
        }
    }
}
