using Moda.Common.Application.Dtos;
using Moda.Common.Application.Requests.WorkManagement.Interfaces;

namespace Moda.Work.Application.Workflows.Dtos;
public sealed record WorkflowDto : IMapFrom<Workflow>, IWorkflowDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required SimpleNavigationDto Ownership { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<WorkflowSchemeDto> Schemes { get; init; } = [];


    IReadOnlyList<IWorkflowSchemeDto> IWorkflowDto.Schemes => Schemes;
}
