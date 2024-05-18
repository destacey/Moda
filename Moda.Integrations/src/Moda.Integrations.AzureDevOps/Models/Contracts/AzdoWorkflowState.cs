using Moda.Common.Application.Interfaces.ExternalWork;
using Moda.Common.Domain.Enums.Work;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public sealed record AzdoWorkflowState : IExternalWorkflowState
{
    public required string StatusName { get; set; }
    public WorkStatusCategory Category { get; set; }
    public int Order { get; set; }
    public bool IsActive { get; set; }
}
