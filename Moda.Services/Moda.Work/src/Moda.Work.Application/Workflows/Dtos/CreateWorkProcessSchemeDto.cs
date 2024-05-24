using Moda.Common.Application.Requests.WorkManagement.Interfaces;

namespace Moda.Work.Application.Workflows.Dtos;
public sealed record CreateWorkProcessSchemeDto : ICreateWorkProcessScheme
{
    public required string WorkTypeName { get; set; }
    public bool WorkTypeIsActive { get; set; }
    public Guid WorkflowId { get; set; }

    public static CreateWorkProcessSchemeDto Create(string workTypeName, bool workTypeIsActive, Guid workflowId)
    {
        return new CreateWorkProcessSchemeDto
        {
            WorkTypeName = workTypeName,
            WorkTypeIsActive = workTypeIsActive,
            WorkflowId = workflowId
        };
    }
}
