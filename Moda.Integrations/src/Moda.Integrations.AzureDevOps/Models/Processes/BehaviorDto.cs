using Ardalis.GuardClauses;
using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums;
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
    /// Returns a list of backlog levels that are requirement level or higher.
    /// </summary>
    /// <param name="behaviors"></param>
    /// <returns></returns>
    public static IList<IExternalBacklogLevel> ToIExternalBacklogLevels(this List<BehaviorDto> behaviors)
    {
        IList<IExternalBacklogLevel> backlogLevels = [];
        foreach (var behavior in behaviors.Where(b => b.Rank > 0))
        {
            Guard.Against.Null(behavior.Inherits, nameof(behavior.Inherits));
            Guard.Against.NullOrEmpty(behavior.Inherits["behaviorRefName"]?.ToString(), "behaviorRefName");

            BacklogCategory? category = MapBehaviorToBacklogCategory(behavior.ReferenceName)
                ?? MapBehaviorToBacklogCategory(behavior.Inherits["behaviorRefName"].ToString()!);
            Guard.Against.Null(category, nameof(category));

            backlogLevels.Add(new AzdoBacklogLevel
            {
                Id = behavior.ReferenceName,
                Name = behavior.Name,
                Description = behavior.Description,
                Rank = behavior.Rank,
                BacklogCategory = category.Value
            });
        }

        return backlogLevels;

        static BacklogCategory? MapBehaviorToBacklogCategory(string behavior)
        {
            Guard.Against.Null(behavior, nameof(behavior));

            return behavior switch
            {
                "System.PortfolioBacklogBehavior" => BacklogCategory.Portfolio,
                "System.RequirementBacklogBehavior" => BacklogCategory.Requirement,
                "System.TaskBacklogBehavior" => BacklogCategory.Task,
                _ => null
            };
        }
    }
}
