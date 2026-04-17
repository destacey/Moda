using Wayd.Common.Domain.Enums.Work;
using Wayd.Work.Domain.Interfaces;

namespace Wayd.Work.Application.Workflows.Dtos;

public sealed record CreateWorkflowSchemeDto : ICreateWorkflowScheme
{
    public int WorkStatusId { get; set; }
    public WorkStatusCategory Category { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }

    public static CreateWorkflowSchemeDto Create(int workStatusId, WorkStatusCategory category, int order, bool isActive)
    {
        return new CreateWorkflowSchemeDto
        {
            WorkStatusId = workStatusId,
            Category = category,
            Order = order,
            IsActive = isActive
        };
    }
}
