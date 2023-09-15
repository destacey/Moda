using Moda.Goals.Domain.Models;

namespace Moda.Goals.Application.Persistence;
public interface IGoalsDbContext : IModaDbContext
{
    DbSet<Objective> Objectives { get; }
}
