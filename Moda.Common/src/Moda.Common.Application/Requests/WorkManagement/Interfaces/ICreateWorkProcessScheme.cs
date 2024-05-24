namespace Moda.Common.Application.Requests.WorkManagement.Interfaces;
public interface ICreateWorkProcessScheme
{
    string WorkTypeName { get; set; }
    bool WorkTypeIsActive { get; set; }
    Guid WorkflowId { get; set; }
}
