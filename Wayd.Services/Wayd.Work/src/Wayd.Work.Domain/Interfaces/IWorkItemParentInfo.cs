using Wayd.Common.Domain.Enums.Work;

namespace Wayd.Work.Domain.Interfaces;

public interface IWorkItemParentInfo
{
    Guid Id { get; }
    int? ExternalId { get; }
    WorkTypeTier Tier { get; }
    int LevelOrder { get; }
    Guid? ProjectId { get; }
}
