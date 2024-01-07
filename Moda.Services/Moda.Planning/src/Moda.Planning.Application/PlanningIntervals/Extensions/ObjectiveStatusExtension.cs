using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.PlanningIntervals.Extensions;
public static class ObjectiveStatusExtension
{
    public static Common.Domain.Enums.Goals.ObjectiveStatus ToGoalObjectiveStatus(this ObjectiveStatus status)
    {
        return status switch
        {
            ObjectiveStatus.NotStarted => Common.Domain.Enums.Goals.ObjectiveStatus.NotStarted,
            ObjectiveStatus.InProgress => Common.Domain.Enums.Goals.ObjectiveStatus.InProgress,
            ObjectiveStatus.Completed => Common.Domain.Enums.Goals.ObjectiveStatus.Completed,
            ObjectiveStatus.Canceled => Common.Domain.Enums.Goals.ObjectiveStatus.Canceled,
            ObjectiveStatus.Missed => Common.Domain.Enums.Goals.ObjectiveStatus.Missed,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
