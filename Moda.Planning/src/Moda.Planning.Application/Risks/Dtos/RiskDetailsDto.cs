using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.Risks.Dtos;
public class RiskDetailsDto : IMapFrom<Risk>
{
    public Guid Id { get; set; }
    public int LocalId { get; set; }
    public string Summary { get; set; } = default!;
    public string? Description { get; set; }
    public Guid TeamId { get; set; }
    public Instant ReportedOn { get; set; }
    public Guid ReportedBy { get; set; }
    public RiskStatus Status { get; set; }
    public RiskCategory Category { get; set; }
    public RiskGrade Impact { get; set; }
    public RiskGrade Likelihood { get; set; }
    public RiskGrade Exposure { get; set; }
    public Guid? AssigneeId { get; set; }
    public LocalDate? FollowUpDate { get; set; }
    public string? Response { get; set; }
    public Instant? ClosedDate { get; set; }
}
