using Moda.Common.Application.Dtos;

namespace Moda.Common.Application.Requests.WorkManagement.Interfaces;
public interface IWorkflowDto
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
    string? Description { get; }
    SimpleNavigationDto Ownership { get; }
    bool IsActive { get; }
    IReadOnlyList<IWorkflowSchemeDto> Schemes { get; }
}
