using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;

namespace Moda.ProjectPortfolioManagement.Domain.Models;

public static class StrategicThemeTagManager<T> where T : class
{
    public static Result AddStrategicThemeTag(HashSet<StrategicThemeTag<T>> tags, Guid objectId, Guid strategicThemeId, string objectType)
    {
        Guard.Against.Default(objectId, nameof(objectId));
        Guard.Against.Default(strategicThemeId, nameof(strategicThemeId));
        Guard.Against.NullOrWhiteSpace(objectType, nameof(objectType));

        if (tags.Any(t => t.StrategicThemeId == strategicThemeId))
        {
            return Result.Failure($"The strategic theme is already associated to this {objectType}.");
        }

        tags.Add(new StrategicThemeTag<T>(objectId, strategicThemeId));

        return Result.Success();
    }

    public static Result RemoveStrategicThemeTag(HashSet<StrategicThemeTag<T>> tags, Guid strategicThemeId, string objectType)
    {
        Guard.Against.Default(strategicThemeId, nameof(strategicThemeId));
        Guard.Against.NullOrWhiteSpace(objectType, nameof(objectType));

        var tag = tags.FirstOrDefault(t => t.StrategicThemeId == strategicThemeId);
        if (tag is null)
        {
            return Result.Failure($"The strategic theme is not associated to this {objectType}.");
        }

        tags.Remove(tag);
        return Result.Success();
    }

    public static Result UpdateTags(HashSet<StrategicThemeTag<T>> tags, Guid objectId, HashSet<Guid> updatedTags, string objectType)
    {
        Guard.Against.Default(objectId, nameof(objectId));
        Guard.Against.NullOrWhiteSpace(objectType, nameof(objectType));

        var currentTags = tags.Select(t => t.StrategicThemeId).ToHashSet();

        var tagsToAdd = updatedTags.Where(t => !currentTags.Contains(t)).ToList();
        foreach (var tag in tagsToAdd)
        {
            var result = AddStrategicThemeTag(tags, objectId, tag, objectType);
            if (result.IsFailure)
            {
                return result;
            }
        }

        var tagsToRemove = tags.Where(t => !updatedTags.Contains(t.StrategicThemeId)).ToList();
        foreach (var tag in tagsToRemove)
        {
            var result = RemoveStrategicThemeTag(tags, tag.StrategicThemeId, objectType);
            if (result.IsFailure)
            {
                return result;
            }
        }

        return Result.Success();
    }
}
