using Moda.Common.Domain.Models.Goals;

namespace Moda.Goals.Application.Persistence;

public interface IGoalsDbContext : IModaDbContext
{
    DbSet<Objective> Objectives { get; }
}
