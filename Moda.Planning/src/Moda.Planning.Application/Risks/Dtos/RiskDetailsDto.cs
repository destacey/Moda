using Moda.Common.Application.Dtos;
using Moda.Common.Extensions;
using Moda.Planning.Application.Models;

namespace Moda.Planning.Application.Risks.Dtos;
public class RiskDetailsDto : IMapFrom<Risk>
{
    public Guid Id { get; set; }
    public int LocalId { get; set; }
    public string Summary { get; set; } = default!;
    public string? Description { get; set; }
    public TeamNavigationDto Team { get; set; } = default!;
    public Instant ReportedOn { get; set; }
    public NavigationDto ReportedBy { get; set; } = default!;
    public required string Status { get; set; }
    public required string Category { get; set; }
    public required string Impact { get; set; }
    public required string Likelihood { get; set; }
    public required string Exposure { get; set; }
    public NavigationDto? Assignee { get; set; }
    public LocalDate? FollowUpDate { get; set; }
    public string? Response { get; set; }
    public Instant? ClosedDate { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Risk, RiskDetailsDto>()
            .Map(dest => dest.Status, src => src.Status.GetDisplayName())
            .Map(dest => dest.Category, src => src.Category.GetDisplayName())
            .Map(dest => dest.Impact, src => src.Impact.GetDisplayName())
            .Map(dest => dest.Likelihood, src => src.Likelihood.GetDisplayName())
            .Map(dest => dest.Exposure, src => src.Exposure.GetDisplayName());
    }
}
