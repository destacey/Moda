namespace Moda.Planning.Application.Persistence;
public interface IPlanningDbContext : IModaDbContext
{
    DbSet<ProgramIncrement> ProgramIncrements { get; }
    DbSet<ProgramIncrementObjective> ProgramIncrementObjectives { get; }
    DbSet<Risk> Risks { get; }
    DbSet<PlanningTeam> PlanningTeams { get; }
}
