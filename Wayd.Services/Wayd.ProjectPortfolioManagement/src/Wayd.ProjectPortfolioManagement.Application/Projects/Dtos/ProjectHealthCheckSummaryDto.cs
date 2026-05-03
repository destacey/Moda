using Wayd.Common.Application.Dtos;

namespace Wayd.ProjectPortfolioManagement.Application.Projects.Dtos;

public sealed record ProjectHealthCheckSummaryDto
{
    public Guid Id { get; set; }
    public required SimpleNavigationDto Status { get; set; }
}
