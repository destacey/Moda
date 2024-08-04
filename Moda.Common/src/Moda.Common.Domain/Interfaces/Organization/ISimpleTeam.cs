using Moda.Common.Domain.Enums.Organization;

namespace Moda.Common.Domain.Interfaces.Organization;
public interface ISimpleTeam
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
    string Code { get; }
    TeamType Type { get; }
    bool IsActive { get; }
}