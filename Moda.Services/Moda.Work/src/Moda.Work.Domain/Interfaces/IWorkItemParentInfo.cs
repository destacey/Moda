using Moda.Common.Domain.Enums.Work;

namespace Moda.Work.Domain.Interfaces;
public interface IWorkItemParentInfo
{
    Guid Id { get; }
    int? ExternalId { get; }
    WorkTypeTier Tier { get; }
    int LevelOrder { get; }
    Guid? ProjectId { get; }
}
