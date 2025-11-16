using Moda.Common.Domain.Enums.Work;

namespace Moda.Common.Domain.Extensions;
public static class DependencyStateExtensions
{
    public static DependencyState FromStatusCategoryString(string statusCategory)
    {
        return statusCategory switch
        {
            "Proposed" => DependencyState.ToDo,
            "Active" => DependencyState.InProgress,
            "Done" => DependencyState.Done,
            "Removed" => DependencyState.Done,
            _ => throw new ArgumentOutOfRangeException(nameof(statusCategory), statusCategory, null)
        };
    }

    public static WorkStatusCategory ToWorkStatusCategory(this DependencyState state)
    {
        return state switch
        {
            DependencyState.ToDo => WorkStatusCategory.Proposed,
            DependencyState.InProgress => WorkStatusCategory.Active,
            DependencyState.Done => WorkStatusCategory.Done,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
}
