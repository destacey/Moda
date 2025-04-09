namespace Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

public class StrategicInitiativeProject
{
    private StrategicInitiativeProject() { }

    private StrategicInitiativeProject(Guid strategicInitiativeId, Guid projectId)
    {
        StrategicInitiativeId = strategicInitiativeId;
        ProjectId = projectId;
    }

    public Guid StrategicInitiativeId { get; private init; }

    public StrategicInitiative? StrategicInitiative { get; private set; }

    public Guid ProjectId { get; private init; }

    public Project? Project { get; private set; }

    internal static StrategicInitiativeProject Create(Guid strategicInitiativeId, Guid projectId)
    {
        return new StrategicInitiativeProject(strategicInitiativeId, projectId);
    }
}
