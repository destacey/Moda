namespace Moda.Common.Application.Requests.WorkManagement.Interfaces;
public interface IWorkProcessSchemeDto
{
    Guid Id { get; }
    IWorkTypeDto WorkType { get; }
    IWorkflowDto? Workflow { get; }
    bool IsActive { get; }
}
