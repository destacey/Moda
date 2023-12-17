namespace Moda.Planning.Application.Persistence;
public interface IPlanningDbContext : IModaDbContext
{
    DbSet<PlanningInterval> PlanningIntervals { get; }
    DbSet<PlanningIntervalObjective> PlanningIntervalObjectives { get; }
    DbSet<Risk> Risks { get; }
    DbSet<PlanningTeam> PlanningTeams { get; }
    DbSet<SimpleHealthCheck> PlanningHealthChecks { get; }
}
