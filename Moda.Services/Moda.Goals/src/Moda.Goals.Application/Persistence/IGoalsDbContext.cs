using Wayd.Common.Domain.Models.Goals;

namespace Wayd.Goals.Application.Persistence;

public interface IGoalsDbContext : IModaDbContext
{
    DbSet<Objective> Objectives { get; }
}
