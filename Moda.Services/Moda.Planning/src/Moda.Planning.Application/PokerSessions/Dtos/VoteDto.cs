using Moda.Common.Application.Dtos;
using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.PokerSessions.Dtos;

public class VoteDto : IMapFrom<PokerVote>
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public string ParticipantName { get; set; } = default!;
    public string Value { get; set; } = default!;
    public Instant SubmittedOn { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<PokerVote, VoteDto>()
            .Map(dest => dest.ParticipantName, src => src.Participant == null ? string.Empty : src.Participant.Name.FullName);
    }
}
