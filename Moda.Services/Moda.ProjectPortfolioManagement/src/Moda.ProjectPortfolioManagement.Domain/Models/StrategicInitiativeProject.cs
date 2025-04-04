﻿namespace Moda.ProjectPortfolioManagement.Domain.Models;

public class StrategicInitiativeProject
{
    private StrategicInitiativeProject() { }

    private StrategicInitiativeProject(Guid strategicInitiativeId, Guid projectId)
    {
        StrategicInitiativeId = strategicInitiativeId;
        ProjectId = projectId;
    }

    public Guid StrategicInitiativeId { get; private init; }

    public StrategicInitiative? StrategicInitiative { get; set; }

    public Guid ProjectId { get; private init; }

    public Project? Project { get; set; }

    internal static StrategicInitiativeProject Create(Guid strategicInitiativeId, Guid projectId)
    {
        return new StrategicInitiativeProject(strategicInitiativeId, projectId);
    }
}
