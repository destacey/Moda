using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Extensions;
using Moda.Planning.Domain.Models.PlanningPoker;

namespace Moda.Planning.Application.PokerSessions.Dtos;

public sealed record PokerSessionListDto : IMapFrom<PokerSession>
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public required string Status { get; set; }
    public EmployeeNavigationDto? Facilitator { get; set; }
    public int RoundCount { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<PokerSession, PokerSessionListDto>()
            .Map(dest => dest.Status, src => src.Status.GetDisplayName())
            .Map(dest => dest.RoundCount, src => src.Rounds.Count);
    }
}
