using Moda.Common.Domain.Enums.Organization;
using NodaTime;

namespace Moda.Common.Domain.Interfaces.Organization;
public interface ISimpleTeam
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
    string Code { get; }
    TeamType Type { get; }
    bool IsActive { get; }
    Instant? Deleted { get; }
    Guid? DeletedBy { get; }
    bool IsDeleted { get; }
}