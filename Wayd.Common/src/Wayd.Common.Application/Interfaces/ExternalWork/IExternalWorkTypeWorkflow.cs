namespace Wayd.Common.Application.Interfaces.ExternalWork;

public interface IExternalWorkTypeWorkflow : IExternalWorkType
{
    IList<IExternalWorkflowState> WorkflowStates { get; }
}
