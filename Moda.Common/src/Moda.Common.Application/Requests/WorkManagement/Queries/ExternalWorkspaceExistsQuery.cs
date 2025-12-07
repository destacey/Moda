namespace Moda.Common.Application.Requests.WorkManagement.Queries;

public sealed record ExternalWorkspaceExistsQuery(string ExternalId) : IQuery<bool>;
