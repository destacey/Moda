﻿using Moda.Common.Domain.Enums.Work;
using Moda.Work.Application.WorkItems.Dtos;

namespace Moda.Work.Application.WorkItems;
internal sealed class WorkItemProgressStateBuilder(IWorkDbContext workDbContext, IQueryable<WorkItem> workItemsQuery)
{
    private readonly IWorkDbContext _workDbContext = workDbContext;
    private readonly IQueryable<WorkItem> _workItemsQuery = workItemsQuery;

    public async Task<List<WorkItemProgressStateDto>> Build(CancellationToken cancellationToken)
    {
        var removedCategory = WorkStatusCategory.Removed; // temporary filter until we get the date from history
        var requirementTier = WorkTypeTier.Requirement;
        var portfolioTier = WorkTypeTier.Portfolio;

        // TODO: should work types in the Task or Other Tier be excluded from the rollup?
        var initialWorkItems = await _workItemsQuery
            .Where(w => w.StatusCategory != removedCategory)
            .Where(w => w.Type.Level != null && (w.Type.Level.Tier == requirementTier || w.Type.Level.Tier == portfolioTier))
            .ProjectToType<WorkItemProgressStateDto>()
            .ToListAsync(cancellationToken);

        if (initialWorkItems.Count == 0 || !initialWorkItems.Any(w => w.Tier == portfolioTier))
            return initialWorkItems;

        var portfolioItems = (await _workDbContext.WorkTypeHierarchies
            .SelectMany(h => h.Levels.Where(l => l.Tier == WorkTypeTier.Portfolio))
            .Select(w => w.Order)
            .Distinct()
            .ToArrayAsync(cancellationToken))
            .OrderBy(l => l)
            .ToDictionary(l => l, l => new List<Guid>());

        var rollupItems = new List<WorkItemProgressStateDto>();
        var rollupItemIds = new HashSet<Guid>();
        foreach (var item in initialWorkItems)
        {
            if (item.Tier == WorkTypeTier.Portfolio)
            {
                UpdatePortfolioItems(portfolioItems, item);
            }
            else if (!rollupItemIds.Contains(item.Id))
            {
                rollupItems.Add(item);
                rollupItemIds.Add(item.Id);
            }
        }

        var startingLevel = portfolioItems.Keys.Min();
        var endingLevel = portfolioItems.Keys.Max();

        if (portfolioItems.Count != 0)
        {
            for (var i = startingLevel; i < (endingLevel + 1); i++)
            {
                // this assumes that the levels are in order and there are no gaps in the levels.
                if (portfolioItems[i].Count == 0)
                    continue;

                var levelItems = await _workDbContext.WorkItems
                    .Where(w => w.StatusCategory != removedCategory)  // temporary filter until we get the date from history
                    .Where(w => w.ParentId.HasValue && portfolioItems[i].Contains(w.ParentId.Value))
                    .ProjectToType<WorkItemProgressStateDto>()
                    .ToListAsync(cancellationToken);

                foreach (var item in levelItems)
                {
                    if (item.Tier == WorkTypeTier.Portfolio)
                    {
                        if (item.LevelOrder <= i)
                            continue;  // skip items that are not in the next level.  This should not happen, but just in case. Work Items shouldn't have parents in a higher level.

                        UpdatePortfolioItems(portfolioItems, item);
                    }
                    else if (!rollupItemIds.Contains(item.Id))
                    {
                        rollupItems.Add(item);
                        rollupItemIds.Add(item.Id);
                    }
                }
            }
        }

        return rollupItems;

        static void UpdatePortfolioItems(Dictionary<int, List<Guid>> portfolioItems, WorkItemProgressStateDto item)
        {
            if (portfolioItems.TryGetValue(item.LevelOrder, out var values))
            {
                if (!values.Any(w => w == item.Id))
                {
                    values.Add(item.Id);
                }
            }
            else
            {
                portfolioItems.Add(item.LevelOrder, [item.Id]);
            }
        }
    }
}
