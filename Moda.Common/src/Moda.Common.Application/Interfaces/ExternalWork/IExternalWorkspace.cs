namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkspace
{
    Guid Id { get; }
    string Name { get; }
    string Abbreviation { get; }
    string? Description { get; }
}
