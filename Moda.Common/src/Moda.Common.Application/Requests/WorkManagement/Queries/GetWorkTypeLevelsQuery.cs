using Moda.Common.Application.Requests.WorkManagement.Interfaces;

namespace Moda.Common.Application.Requests.WorkManagement.Queries;
public sealed record GetWorkTypeLevelsQuery() : IQuery<IReadOnlyList<IWorkTypeLevelDto>>;
