using Moda.Common.Application.Dtos;

namespace Moda.Common.Application.Requests.WorkManagement.Interfaces;
public interface IWorkTypeLevelDto
{
    int Id { get; }
    string Name { get; }
    string? Description { get; }
    SimpleNavigationDto Tier { get; }
    int Order { get; }
}
