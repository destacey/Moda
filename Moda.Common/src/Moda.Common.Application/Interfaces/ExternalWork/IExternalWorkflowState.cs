using Moda.Common.Domain.Enums.Work;

namespace Moda.Common.Application.Interfaces.ExternalWork;

public interface IExternalWorkflowState
{
    string StatusName { get; }
    WorkStatusCategory Category { get; }
    int Order { get; }
    bool IsActive { get; }
}
