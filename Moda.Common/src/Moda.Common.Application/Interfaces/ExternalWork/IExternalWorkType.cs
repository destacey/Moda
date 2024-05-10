namespace Moda.Common.Application.Interfaces.ExternalWork;
public interface IExternalWorkType
{
    string Id { get; }
    string Name { get; }
    string? Description { get; }
    string BacklogLevelId { get; }
    bool IsActive { get; }
}
