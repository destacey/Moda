using Moda.Common.Application.Dtos;
using Moda.Common.Extensions;
using Moda.Planning.Application.Models;

namespace Moda.Planning.Application.Risks.Dtos;
public class RiskListDto : IMapFrom<Risk>
{
    public Guid Id { get; set; }
    public int LocalId { get; set; }
    public string Summary { get; set; } = default!;
    public PlanningTeamNavigationDto? Team { get; set; } = default!;
    public Instant ReportedOn { get; set; }
    public required string Status { get; set; }
    public required string Category { get; set; }
    public required string Exposure { get; set; }
    public NavigationDto? Assignee { get; set; }
    public LocalDate? FollowUpDate { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Risk, RiskListDto>()
            .Map(dest => dest.Status, src => src.Status.GetDisplayName())
            .Map(dest => dest.Category, src => src.Category.GetDisplayName())
            .Map(dest => dest.Exposure, src => src.Exposure.GetDisplayName())
            .Map(dest => dest.Assignee, src => src.Assignee == null ? null : NavigationDto.Create(src.Assignee.Id, src.Assignee.LocalId, src.Assignee.Name.FullName));
    }
}
