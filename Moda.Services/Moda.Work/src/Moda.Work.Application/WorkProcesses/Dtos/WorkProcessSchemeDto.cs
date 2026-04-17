using Wayd.Common.Application.Requests.WorkManagement.Interfaces;
using Wayd.Work.Application.Workflows.Dtos;
using Wayd.Work.Application.WorkTypes.Dtos;

namespace Wayd.Work.Application.WorkProcesses.Dtos;

public sealed record WorkProcessSchemeDto : IMapFrom<WorkProcessScheme>, IWorkProcessSchemeDto
{
    public Guid Id { get; set; }
    public required WorkTypeDto WorkType { get; set; }
    public required WorkflowDto Workflow { get; set; }
    public bool IsActive { get; set; }

    IWorkTypeDto IWorkProcessSchemeDto.WorkType => WorkType;
    IWorkflowDto IWorkProcessSchemeDto.Workflow => Workflow;
}
