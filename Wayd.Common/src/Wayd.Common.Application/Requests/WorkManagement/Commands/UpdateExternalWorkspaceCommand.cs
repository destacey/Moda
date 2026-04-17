using Wayd.Common.Application.Interfaces.ExternalWork;

namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

public sealed record UpdateExternalWorkspaceCommand(IExternalWorkspaceConfiguration ExternalWorkspace) : ICommand, ILongRunningRequest;
