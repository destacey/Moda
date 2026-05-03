using Wayd.Common.Application.Dtos;
using Wayd.ProjectPortfolioManagement.Domain.Models;

namespace Wayd.ProjectPortfolioManagement.Application.Projects.Dtos;

public sealed record ProjectHealthCheckDto
{
    public Guid Id { get; set; }
    public required SimpleNavigationDto Status { get; set; }
    public Instant ReportedOn { get; set; }
    public Instant Expiration { get; set; }
    public string? Note { get; set; }

    public static ProjectHealthCheckDto? FromCurrent(IEnumerable<ProjectHealthCheck> healthChecks, Instant now)
    {
        var current = healthChecks.FirstOrDefault(h => !h.IsExpired(now));
        if (current is null)
            return null;

        return new ProjectHealthCheckDto
        {
            Id = current.Id,
            Status = SimpleNavigationDto.FromEnum(current.Status),
            ReportedOn = current.ReportedOn,
            Expiration = current.Expiration,
            Note = current.Note
        };
    }
}
