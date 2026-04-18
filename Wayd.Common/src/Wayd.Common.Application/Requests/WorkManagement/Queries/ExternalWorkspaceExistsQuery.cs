namespace Wayd.Common.Application.Requests.WorkManagement.Queries;

public sealed record ExternalWorkspaceExistsQuery(string ExternalId) : IQuery<bool>;
