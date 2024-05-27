using Moda.Common.Application.Dtos;
using Moda.Common.Application.Requests.WorkManagement.Interfaces;

namespace Moda.Work.Application.Workflows.Dtos;
public sealed record WorkflowDto : IMapFrom<Workflow>, IWorkflowDto
{
    public Guid Id { get; set; }
    public int Key { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required SimpleNavigationDto Ownership { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<WorkflowSchemeDto> Schemes { get; set; } = [];


    IReadOnlyList<IWorkflowSchemeDto> IWorkflowDto.Schemes => Schemes;
}
