using Moda.Common.Domain.Enums.StrategicManagement;

namespace Moda.Common.Domain.Interfaces.StrategicManagement;
public interface IStrategicThemeData
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
    string Description { get; }
    StrategicThemeState State { get; }
}
