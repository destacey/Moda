using Moda.Common.Application.Dtos;

namespace Moda.Common.Application.Requests.WorkManagement.Interfaces;
public interface IWorkflowSchemeDto
{
    Guid Id { get; }
    IWorkStatusDto WorkStatus { get; }
    SimpleNavigationDto WorkStatusCategory { get; }
    int Order { get; }
    bool IsActive { get; }
}
