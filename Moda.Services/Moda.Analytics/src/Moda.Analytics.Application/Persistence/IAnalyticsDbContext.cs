namespace Moda.Analytics.Application.Persistence;

public interface IAnalyticsDbContext : IModaDbContext
{
    DbSet<AnalyticsView> AnalyticsViews { get; }
    DbSet<WorkItem> WorkItems { get; }
}
