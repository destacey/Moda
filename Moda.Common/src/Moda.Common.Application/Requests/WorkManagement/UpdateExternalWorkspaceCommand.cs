using Moda.Common.Application.Interfaces.ExternalWork;

namespace Moda.Common.Application.Requests.WorkManagement;
public sealed record UpdateExternalWorkspaceCommand(IExternalWorkspaceConfiguration ExternalWorkspace) : ICommand;
