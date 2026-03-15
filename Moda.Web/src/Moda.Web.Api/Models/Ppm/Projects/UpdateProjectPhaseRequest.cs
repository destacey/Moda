using Moda.ProjectPortfolioManagement.Application.Projects.Commands;

namespace Moda.Web.Api.Models.Ppm.Projects;

public sealed record UpdateProjectPhaseRequest
{
    public required string Description { get; init; }
    public int Status { get; init; }
    public LocalDate? PlannedStart { get; init; }
    public LocalDate? PlannedEnd { get; init; }
    public decimal Progress { get; init; }

    public UpdateProjectPhaseCommand ToCommand(Guid projectId, Guid phaseId)
    {
        return new UpdateProjectPhaseCommand(projectId, phaseId, Description, Status, PlannedStart, PlannedEnd, Progress);
    }
}
