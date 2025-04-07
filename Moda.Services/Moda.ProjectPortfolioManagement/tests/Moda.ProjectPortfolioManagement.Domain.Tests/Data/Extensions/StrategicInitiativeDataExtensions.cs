using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moda.ProjectPortfolioManagement.Domain.Models;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;
using Moda.Tests.Shared;
using Moda.Tests.Shared.Extensions;

namespace Moda.ProjectPortfolioManagement.Domain.Tests.Data.Extensions;
public static class StrategicInitiativeDataExtensions
{
    public static StrategicInitiative AddKpis(this StrategicInitiative strategicInitiative, int count)
    {
        if (count <= 0) return strategicInitiative;

        var kpiFaker = new StrategicInitiativeKpiFaker();

        var kpisList = GenericExtensions.GetPrivateHashSet<StrategicInitiativeKpi>(strategicInitiative, "_kpis");

        for (int i = 0; i < count; i++)
        {
            var kpi = kpiFaker.WithData(strategicInitiativeId: strategicInitiative.Id).Generate();
            kpisList.Add(kpi);
        }

        return strategicInitiative;
    }

    public static StrategicInitiative AddProjects(this StrategicInitiative strategicInitiative, int count, TestingDateTimeProvider dateTimeProvider)
    {
        if (count <= 0) return strategicInitiative;

        var projectFaker = new ProjectFaker();

        var projectsList = GenericExtensions.GetPrivateHashSet<Project>(strategicInitiative, "_projects");

        // TODO: add logic based on the current status of the strategic initiative
        for (int i = 0; i < count - 1; i++)
        {
            var project = projectFaker.AsActive(dateTimeProvider, strategicInitiative.Id);
            projectsList.Add(project);
        }

        // Add a proposed strategic initiative as the last one
        var lastProject = projectFaker.AsProposed(dateTimeProvider, strategicInitiative.Id);
        projectsList.Add(lastProject);

        return strategicInitiative;
    }
}
