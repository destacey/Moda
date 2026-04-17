using Wayd.Common.Application.Dtos;
using Wayd.Common.Application.Requests.WorkManagement.Interfaces;
using Wayd.Work.Application.WorkStatuses.Dtos;

namespace Wayd.Work.Application.Workflows.Dtos;

public sealed record WorkflowSchemeDto : IMapFrom<WorkflowScheme>, IWorkflowSchemeDto
{
    public Guid Id { get; set; }
    public required WorkStatusDto WorkStatus { get; set; }
    public required SimpleNavigationDto WorkStatusCategory { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }

    IWorkStatusDto IWorkflowSchemeDto.WorkStatus => WorkStatus;
}
