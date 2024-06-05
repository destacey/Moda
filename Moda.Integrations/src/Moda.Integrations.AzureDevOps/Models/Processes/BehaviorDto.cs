using Ardalis.GuardClauses;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums.Work;
using Moda.Integrations.AzureDevOps.Models.Contracts;

namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record BehaviorDto
{
    public required string ReferenceName { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Rank { get; set; }
    public required string Customization { get; set; }
    public Dictionary<string, object>? Inherits { get; set; }
}

internal static class BehaviorDtoExtensions
{

    /// <summary>
    /// Returns a list of work type levels that are requirement level or higher.
    /// </summary>
    /// <param name="behaviors"></param>
    /// <returns></returns>
    public static IList<IExternalWorkTypeLevel> ToIExternalWorkTypeLevels(this List<BehaviorDto> behaviors)
    {
        IList<IExternalWorkTypeLevel> levels = [];
        foreach (var behavior in behaviors.Where(b => b.Rank > 0))
        {
            Guard.Against.Null(behavior.Inherits, nameof(behavior.Inherits));
            Guard.Against.NullOrEmpty(behavior.Inherits["behaviorRefName"]?.ToString(), "behaviorRefName");

            WorkTypeTier? category = MapBehaviorToWorkTypeTier(behavior.ReferenceName)
                ?? MapBehaviorToWorkTypeTier(behavior.Inherits["behaviorRefName"].ToString()!);
            Guard.Against.Null(category, nameof(category));

            levels.Add(new AzdoWorkTypeLevel
            {
                Id = behavior.ReferenceName,
                Name = behavior.Name,
                Description = behavior.Description,
                Order = behavior.Rank,
                Tier = category.Value
            });
        }

        return levels;

        static WorkTypeTier? MapBehaviorToWorkTypeTier(string behavior)
        {
            Guard.Against.Null(behavior, nameof(behavior));

            return behavior switch
            {
                "System.PortfolioBacklogBehavior" => WorkTypeTier.Portfolio,
                "System.RequirementBacklogBehavior" => WorkTypeTier.Requirement,
                "System.TaskBacklogBehavior" => WorkTypeTier.Task,
                _ => null
            };
        }
    }
}
