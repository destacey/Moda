namespace Moda.Common.Application.Interfaces.Work;
public interface IExternalWorkItem
{
    int Id { get; }
    string Title { get; }
    string WorkType { get; }
    string WorkStatus { get; }
    string? AssignedTo { get; }
    Instant Created { get; }
    string? CreatedBy { get; }
    Instant LastModified { get; }
    string? LastModifiedBy { get; }
    int? Priority { get; }
    double StackRank { get; }
}
