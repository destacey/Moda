using Moda.Common.Application.Interfaces.Work;
using NodaTime;

namespace Moda.Integrations.AzureDevOps.Models.Contracts;
public record AzdoWorkItem : IExternalWorkItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string WorkType { get; set; }
    public required string WorkStatus { get; set; }
    public string? AssignedTo { get; set; }
    public Instant Created { get; set; }
    public string? CreatedBy { get; set; }
    public Instant LastModified { get; set; }
    public string? LastModifiedBy { get; set; }
    public int? Priority { get; set; }
    public double StackRank { get; set; }
}
