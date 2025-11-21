using Moda.Common.Domain.Enums.Organization;
using Moda.Common.Domain.Models.Organizations;

namespace Moda.Common.Domain.Interfaces.Organization;
public interface ISimpleTeam
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
    TeamCode Code { get; }
    TeamType Type { get; }
    bool IsActive { get; }
}