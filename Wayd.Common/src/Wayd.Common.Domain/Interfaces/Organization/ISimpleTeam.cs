using Wayd.Common.Domain.Enums.Organization;
using Wayd.Common.Domain.Models.Organizations;

namespace Wayd.Common.Domain.Interfaces.Organization;

public interface ISimpleTeam
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
    TeamCode Code { get; }
    TeamType Type { get; }
    bool IsActive { get; }
}