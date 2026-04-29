using Wayd.Planning.Domain.Models.Iterations;
using Wayd.Planning.Domain.Models.PlanningPoker;
using Wayd.Planning.Domain.Models.Roadmaps;

namespace Wayd.Planning.Application.Persistence;

public interface IPlanningDbContext : IWaydDbContext
{
    DbSet<Iteration> Iterations { get; }
    DbSet<PlanningIntervalObjective> PlanningIntervalObjectives { get; }
    DbSet<PlanningInterval> PlanningIntervals { get; }
    DbSet<Risk> Risks { get; }
    DbSet<PlanningTeam> PlanningTeams { get; }
    DbSet<PlanningIntervalObjectiveHealthCheck> PlanningIntervalObjectiveHealthChecks { get; }
    DbSet<Roadmap> Roadmaps { get; }
    DbSet<EstimationScale> EstimationScales { get; }
    DbSet<PokerSession> PokerSessions { get; }
}
