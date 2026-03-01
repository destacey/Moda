using Moda.Common.Application.Employees.Dtos;
using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.PokerSessions.Dtos;

public sealed record PokerVoteDto : IMapFrom<PokerVote>
{
    public Guid Id { get; set; }
    public EmployeeNavigationDto? Participant { get; set; }
    public string Value { get; set; } = default!;
    public Instant SubmittedOn { get; set; }
}
