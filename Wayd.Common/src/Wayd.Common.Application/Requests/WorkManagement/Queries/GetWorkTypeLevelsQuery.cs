using Wayd.Common.Application.Requests.WorkManagement.Interfaces;

namespace Wayd.Common.Application.Requests.WorkManagement.Queries;

public sealed record GetWorkTypeLevelsQuery() : IQuery<IReadOnlyList<IWorkTypeLevelDto>>;
