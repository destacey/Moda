namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkspaceConfiguration : IExternalWorkspace
{
    Guid WorkProcessId { get; set; }
}
