using Moda.Work.Domain.Models;

namespace Moda.Work.Domain.Interfaces;

public interface IHasOptionalWorkTeam
{
    Guid? TeamId { get; }
    WorkTeam? Team { get; }
}
