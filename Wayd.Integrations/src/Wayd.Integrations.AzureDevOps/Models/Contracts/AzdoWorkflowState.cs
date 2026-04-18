using Wayd.Common.Application.Interfaces.ExternalWork;
using Wayd.Common.Domain.Enums.Work;

namespace Wayd.Integrations.AzureDevOps.Models.Contracts;

public sealed record AzdoWorkflowState : IExternalWorkflowState
{
    public required string StatusName { get; set; }
    public WorkStatusCategory Category { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
}
