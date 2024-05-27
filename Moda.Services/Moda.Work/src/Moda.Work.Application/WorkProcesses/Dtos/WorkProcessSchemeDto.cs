using Moda.Common.Application.Requests.WorkManagement.Interfaces;
using Moda.Work.Application.Workflows.Dtos;
using Moda.Work.Application.WorkTypes.Dtos;

namespace Moda.Work.Application.WorkProcesses.Dtos;
public sealed record WorkProcessSchemeDto : IMapFrom<WorkProcessScheme>, IWorkProcessSchemeDto
{
    public Guid Id { get; set; }
    public required WorkTypeDto WorkType { get; set; }
    public WorkflowDto? Workflow { get; set; }
    public bool IsActive { get; set; }

    IWorkTypeDto IWorkProcessSchemeDto.WorkType => WorkType;
    IWorkflowDto? IWorkProcessSchemeDto.Workflow => Workflow;
}
