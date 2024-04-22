using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Extensions;
using Moda.Planning.Application.Models;

namespace Moda.Planning.Application.Risks.Dtos;
public class RiskListDto : IMapFrom<Risk>
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Summary { get; set; }
    public PlanningTeamNavigationDto? Team { get; set; }
    public Instant ReportedOn { get; set; }
    public required string Status { get; set; }
    public required string Category { get; set; }
    public required string Exposure { get; set; }
    public EmployeeNavigationDto? Assignee { get; set; }
    public LocalDate? FollowUpDate { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Risk, RiskListDto>()
            .Map(dest => dest.Status, src => src.Status.GetDisplayName())
            .Map(dest => dest.Category, src => src.Category.GetDisplayName())
            .Map(dest => dest.Exposure, src => src.Exposure.GetDisplayName())
            .Map(dest => dest.Assignee, src => src.Assignee == null ? null : EmployeeNavigationDto.From(src.Assignee));
    }
}
