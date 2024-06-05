using Moda.Common.Domain.Enums.Work;

namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkTypeLevel
{
    string Id { get; set; }
    string Name { get; set; }
    string? Description { get; set; }
    WorkTypeTier Tier { get; }
    int Order { get; set; }
}
