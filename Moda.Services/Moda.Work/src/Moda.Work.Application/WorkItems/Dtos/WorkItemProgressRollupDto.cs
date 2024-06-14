using Moda.Common.Domain.Enums.Work;

namespace Moda.Work.Application.WorkItems.Dtos;

/// <summary>
/// Progress data for a collection of work items.  Work items with a status category of Proposed, Active, and Done are counted.  Work items with a status category of Removed are not included
/// </summary>
public record WorkItemProgressRollupDto
{
    public int Proposed { get; protected set; }
    public int Active { get; protected set; }
    public int Done { get; protected set; }
    public int Total => Proposed + Active + Done;

    public static WorkItemProgressRollupDto Create(int proposed, int active, int done)
    {
        return new WorkItemProgressRollupDto
        {
            Proposed = proposed,
            Active = active,
            Done = done
        };
    }

    public static WorkItemProgressRollupDto CreateEmpty()
    {
        return Create(0,0,0);
    }

    public static WorkItemProgressRollupDto Create(List<WorkItemProgressStateDto> workItems)
    {
        int proposed = 0, active = 0, done = 0;

        foreach (var workItem in workItems)
        {
            switch (workItem.StatusCategory)
            {
                case WorkStatusCategory.Proposed:
                    proposed++;
                    break;
                case WorkStatusCategory.Active:
                    active++;
                    break;
                case WorkStatusCategory.Done:
                    done++;
                    break;
            }
        }

        return Create(proposed, active, done);
    }

    public static WorkItemProgressRollupDto Create(List<WorkItemProgressRollupDto> rollups)
    {
        int proposed = 0, active = 0, done = 0;

        foreach (var rollup in rollups)
        {
            proposed += rollup.Proposed;
            active += rollup.Active;
            done += rollup.Done;
        }

        return new WorkItemProgressRollupDto
        {
            Proposed = proposed,
            Active = active,
            Done = done
        };
    }
}
