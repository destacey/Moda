using Wayd.Common.Application.Dtos;

namespace Wayd.Common.Application.Requests.WorkManagement.Interfaces;

public interface IWorkTypeLevelDto
{
    int Id { get; }
    string Name { get; }
    string? Description { get; }
    SimpleNavigationDto Tier { get; }
    int Order { get; }
}
