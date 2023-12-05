namespace Moda.Integrations.AzureDevOps.Models.Processes;

internal sealed record BehaviorDto
{
    public required string ReferenceName { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Rank { get; set; }
    public required string Customization { get; set; }
}

internal static class BehaviorDtoExtensions
{
    public static AzdoBacklogLevel ToAzdoBacklogLevel(this BehaviorDto behavior)
    {
        return new AzdoBacklogLevel
        {
            Id = behavior.ReferenceName,
            Name = behavior.Name,
            Description = behavior.Description,
            Rank = behavior.Rank,
        };
    }

    /// <summary>
    /// Returns a list of backlog levels that are requirement level or higher.
    /// </summary>
    /// <param name="behaviors"></param>
    /// <returns></returns>
    public static List<AzdoBacklogLevel> ToAzdoBacklogLevels(this List<BehaviorDto> behaviors)
    {
        return behaviors
            .Where(b => b.Rank > 0 && b.ReferenceName != "System.TaskBacklogBehavior")
            .Select(b => b.ToAzdoBacklogLevel()).ToList();
    }
}
