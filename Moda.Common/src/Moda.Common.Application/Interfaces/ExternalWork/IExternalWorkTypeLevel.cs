using Wayd.Common.Domain.Enums.Work;

namespace Wayd.Common.Application.Interfaces.ExternalWork;

public interface IExternalWorkTypeLevel
{
    string Id { get; set; }
    string Name { get; set; }
    string? Description { get; set; }
    WorkTypeTier Tier { get; }
    int Order { get; set; }
}
