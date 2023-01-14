using Ardalis.GuardClauses;
using CSharpFunctionalExtensions;
using NodaTime;

namespace Moda.Work.Domain.Models;

/// <summary>
/// This object ensures the integrity and system state of global backlog levels.
/// </summary>
public class BacklogLevelScheme : BaseAuditableEntity<Guid>
{
    private readonly List<BacklogLevel> _backlogLevels = new();

    private BacklogLevelScheme() { }

    private BacklogLevelScheme(Instant timestamp)
    {
        _backlogLevels.AddRange(GetSystemDefaultBacklogLevels(timestamp));
    }

    /// <summary>
    /// List of child backlog levels.
    /// </summary>
    public IReadOnlyList<BacklogLevel> BacklogLevels => _backlogLevels.AsReadOnly();

    /// <summary>
    /// Add a backlog level to the scheme.  The backlog level category must be a Portfolio BacklogCategory.
    /// </summary>
    /// <param name="backlogLevel"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result AddPortfolioBacklogLevel(BacklogLevel backlogLevel, Instant timestamp)
    {
        Guard.Against.Null(backlogLevel);

        if (backlogLevel.Category != BacklogCategory.Portfolio)
            return Result.Failure("Backlog level must be of category Portfolio.");

        if (_backlogLevels.Any(l => l.Name == backlogLevel.Name))
            return Result.Failure($"A backlog level with the name '{backlogLevel.Name}' already exists.");
        
        _backlogLevels.Add(backlogLevel);
        AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));
        return Result.Success();
    }

    /// <summary>
    /// Update an existing backlog level.  The backlog level category must be a Portfolio BacklogCategory.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <param name="rank"></param>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Result UpdatePortfolioBacklogLevel(int id,
            string name,
            string? description,
            int rank,
            Instant timestamp)
    {
        var backlogLevel = _backlogLevels.FirstOrDefault(x => x.Id == id);
        if (backlogLevel is null)
            return Result.Failure<int>("Backlog Level not found.");
        
        if (backlogLevel.Category != BacklogCategory.Portfolio)
            return Result.Failure("Backlog level must be of category Portfolio.");

        if (_backlogLevels.Where(l => l.Id != id).Any(l => l.Name == backlogLevel.Name))
            return Result.Failure($"A backlog level with the name '{backlogLevel.Name}' already exists.");

        var result = backlogLevel.Update(name, description, rank, timestamp);
        if (result.IsFailure)
            return Result.Failure<int>(result.Error);
        
        AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));
        return Result.Success();
    }

    public Result Reinitialize(Instant timestamp)
    {
        try
        {
            bool hasChanged = false;
            List<BacklogLevel> defaultBacklogLevels = GetSystemDefaultBacklogLevels(timestamp);

            foreach (var defaultBacklogLevel in defaultBacklogLevels)
            {
                var existing = _backlogLevels.FirstOrDefault(x => x.Name == defaultBacklogLevel.Name && x.Ownership == Ownership.System);
                if (existing is not null)
                {
                    // Update existing backlog level if it has changed
                    if (existing.Description != defaultBacklogLevel.Description
                        || existing.Rank != defaultBacklogLevel.Rank)
                    {
                        var result = existing.Update(
                            defaultBacklogLevel.Name, 
                            defaultBacklogLevel.Description, 
                            defaultBacklogLevel.Rank,
                            timestamp);

                        if (result.IsSuccess)
                            hasChanged = true;
                    }
                }
                else
                {
                    // Add new backlog level since it does not exist
                    _backlogLevels.Add(defaultBacklogLevel);
                    hasChanged = true;
                }
            }

            if (hasChanged)
                AddDomainEvent(EntityUpdatedEvent.WithEntity(this, timestamp));

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.ToString());
        }
    }

    /// <summary>Initializes the global BacklogLevelScheme.  This should only ever called once.</summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns></returns>
    public static BacklogLevelScheme Initialize(Instant timestamp)
    {
        BacklogLevelScheme scheme = new(timestamp);

        scheme.AddDomainEvent(EntityCreatedEvent.WithEntity(scheme, timestamp));
        return scheme;
    }

    private static List<BacklogLevel> GetSystemDefaultBacklogLevels(Instant timestamp)
    {
        // backlog levels cannot be removed or have their name changed in this list without changes to the current ReInitialize process
        List<BacklogLevel> backlogLevels = new()
        {
            BacklogLevel.Create("Stories", "Stories backlog level.", BacklogCategory.Requirement, Ownership.System, 1, timestamp),

            BacklogLevel.Create("Tasks", "Tasks backlog level.", BacklogCategory.Task, Ownership.System, 1, timestamp),

            BacklogLevel.Create("Other", "Backlog level for work item types not mapped to one of the primary backlog levels.", BacklogCategory.Other, Ownership.System, 1, timestamp),

            BacklogLevel.Create("Features", "Features backlog level.", BacklogCategory.Portfolio, Ownership.System, 1, timestamp),
            BacklogLevel.Create("Epics", "Epics backlog level.", BacklogCategory.Portfolio, Ownership.System, 2, timestamp)
        };

        return backlogLevels;
    }
}
