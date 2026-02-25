namespace Moda.Analytics.Domain.Models;

public sealed class AnalyticsViewManager
{
    private AnalyticsViewManager() { }

    internal AnalyticsViewManager(AnalyticsView analyticsView, Guid managerId)
    {
        AnalyticsViewId = analyticsView.Id;
        ManagerId = managerId;
    }

    public Guid AnalyticsViewId { get; private init; }

    public Guid ManagerId { get; private init; }
}
