using Moda.Common.Domain.Enums.Work;

namespace Moda.Common.Domain.Extensions;
public static class DependencyStatusExtensions
{
    public static DependencyStatus FromStatusCategoryString(string statusCategory)
    {
        return statusCategory switch
        {
            "Proposed" => DependencyStatus.Proposed,
            "Active" => DependencyStatus.Active,
            "Done" => DependencyStatus.Done,
            _ => throw new ArgumentOutOfRangeException(nameof(statusCategory), statusCategory, null)
        };
    }
}
