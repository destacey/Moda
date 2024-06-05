namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkType
{
    string Name { get; }
    string? Description { get; }
    string WorkTypeLevelId { get; }
    bool IsActive { get; }
}
