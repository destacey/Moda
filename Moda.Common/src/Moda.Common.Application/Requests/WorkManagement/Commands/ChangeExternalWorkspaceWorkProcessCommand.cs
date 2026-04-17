namespace Wayd.Common.Application.Requests.WorkManagement.Commands;

/// <summary>
/// Changes the work process assigned to an externally-managed workspace.
/// Used when the external system (e.g., Azure DevOps) reassigns a project to a different process.
/// </summary>
/// <param name="WorkspaceId">The internal Moda workspace ID.</param>
/// <param name="NewWorkProcessExternalId">The external ID of the new work process (e.g., Azure DevOps process GUID).</param>
public sealed record ChangeExternalWorkspaceWorkProcessCommand(Guid WorkspaceId, Guid NewWorkProcessExternalId) : ICommand;
