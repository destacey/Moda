namespace Wayd.Common.Application.Requests.WorkManagement.Queries;

public sealed record ExternalWorkProcessExistsQuery(Guid ExternalId) : IQuery<bool>;