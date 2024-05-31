using Moda.Common.Domain.Enums.Work;

namespace Moda.Common.Application.Requests.WorkManagement.Interfaces;
public interface IWorkTypeLevelDto
{
    int Id { get; }
    string Name { get; }
    string? Description { get; }
    WorkTypeTier Tier { get; }
    int Order { get; }
}
