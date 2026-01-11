using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Domain.Services;

/// <summary>
/// Domain service for calculating Work Breakdown Structure (WBS) codes for project tasks.
/// </summary>
public static class WbsCalculator
{
    /// <summary>
    /// Calculates the WBS code for a specific task based on its position in the hierarchy.
    /// </summary>
    /// <param name="task">The task to calculate the WBS code for.</param>
    /// <param name="allTasks">All tasks in the project.</param>
    /// <returns>The WBS code (e.g., "1.2.1").</returns>
    public static string CalculateWbs(ProjectTask task, IEnumerable<ProjectTask> allTasks)
    {
        var path = new List<int>();
        var current = task;
        var taskList = allTasks.ToList();

        // Build path from current task to root
        while (current is not null)
        {
            var siblings = taskList
                .Where(t => t.ParentId == current.ParentId)
                .OrderBy(t => t.Order)
                .ToList();

            var index = siblings.FindIndex(t => t.Id == current.Id);
            if (index >= 0)
            {
                path.Insert(0, index + 1); // 1-based indexing
            }

            // Navigate to parent
            current = current.ParentId.HasValue
                ? taskList.FirstOrDefault(t => t.Id == current.ParentId)
                : null;
        }

        return string.Join(".", path);
    }

    /// <summary>
    /// Calculates WBS codes for all tasks in a collection.
    /// </summary>
    /// <param name="tasks">The tasks to calculate WBS codes for.</param>
    /// <returns>A dictionary mapping task IDs to their WBS codes.</returns>
    public static Dictionary<Guid, string> CalculateAllWbs(IEnumerable<ProjectTask> tasks)
    {
        var result = new Dictionary<Guid, string>();
        var taskList = tasks.ToList();

        foreach (var task in taskList)
        {
            result[task.Id] = CalculateWbs(task, taskList);
        }

        return result;
    }
}
