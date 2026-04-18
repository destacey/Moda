using Wayd.Common.Domain.Models.Goals;

namespace Wayd.Goals.Application.Persistence;

public interface IGoalsDbContext : IWaydDbContext
{
    DbSet<Objective> Objectives { get; }
}
