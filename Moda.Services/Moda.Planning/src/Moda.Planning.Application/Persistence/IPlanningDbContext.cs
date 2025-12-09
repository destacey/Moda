using Moda.Planning.Domain.Models.Iterations;
using Moda.Planning.Domain.Models.Roadmaps;

namespace Moda.Planning.Application.Persistence;
public interface IPlanningDbContext : IModaDbContext
{
    DbSet<Iteration> Iterations { get; }
    DbSet<PlanningIntervalObjective> PlanningIntervalObjectives { get; }
    DbSet<PlanningInterval> PlanningIntervals { get; }
    DbSet<Risk> Risks { get; }
    DbSet<PlanningTeam> PlanningTeams { get; }
    DbSet<SimpleHealthCheck> PlanningHealthChecks { get; }
    DbSet<Roadmap> Roadmaps { get; }
}
