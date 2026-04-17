using Wayd.Work.Domain.Models;

namespace Wayd.Work.Domain.Interfaces;

public interface IHasOptionalWorkTeam
{
    Guid? TeamId { get; }
    WorkTeam? Team { get; }
}
