using Wayd.Common.Domain.Enums.Work;

namespace Wayd.Work.Domain.Interfaces;

public interface ICreateWorkflowScheme
{
    int WorkStatusId { get; }
    WorkStatusCategory Category { get; }
    int Order { get; }
    bool IsActive { get; }
}
