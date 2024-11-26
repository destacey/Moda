using Moda.Common.Domain.Enums.Work;

namespace Moda.Common.Domain.Extensions;
public static class DependencyStatusExtensions
{
    public static DependencyStatus FromStatusCategoryString(string statusCategory)
    {
        return statusCategory switch
        {
            "Proposed" => DependencyStatus.ToDo,
            "Active" => DependencyStatus.InProgress,
            "Done" => DependencyStatus.Done,
            "Removed" => DependencyStatus.Done,
            _ => throw new ArgumentOutOfRangeException(nameof(statusCategory), statusCategory, null)
        };
    }

    public static WorkStatusCategory ToWorkStatusCategory(this DependencyStatus status)
    {
        return status switch
        {
            DependencyStatus.ToDo => WorkStatusCategory.Proposed,
            DependencyStatus.InProgress => WorkStatusCategory.Active,
            DependencyStatus.Done => WorkStatusCategory.Done,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}
