namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkProcess
{
    Guid Id { get; }
    string Name { get; }
    string? Description { get; }
}
