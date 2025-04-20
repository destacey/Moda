namespace Moda.Common.Domain.Interfaces.Ppm;

public interface ISimpleProject
{
    Guid Id { get; }
    int Key { get; }
    string Name { get; }
}
