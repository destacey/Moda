using Moda.Common.Application.Dtos;
using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Domain.Extensions;

namespace Moda.Work.Application.WorkItems.Dtos;
public sealed record ScopedDependencyDto
{
    public Guid Id { get; set; }
    public required WorkItemDetailsNavigationDto Dependency { get; set; }

    public required string Type { get; set; }

    public required SimpleNavigationDto Status { get; set; }

    public Instant CreatedOn { get; set; }

    public EmployeeNavigationDto? CreatedBy { get; set; }

    public string? Comment { get; set; }

    /// <summary>
    /// Converts a work item link to a scoped dependency.
    /// </summary>
    /// <param name="link"></param>
    /// <param name="workItemKey">The work item key in which the dependencies are scoped to.</param>
    /// <returns></returns>
    public static ScopedDependencyDto From(WorkItemLinkDto link, WorkItemKey workItemKey)
    {
        ArgumentNullException.ThrowIfNull(link);
        ArgumentNullException.ThrowIfNull(workItemKey);
        ArgumentNullException.ThrowIfNull(link.Source);
        ArgumentNullException.ThrowIfNull(link.Target);

        var isOutbound = link.Source.Key == workItemKey;
        var dependency = isOutbound ? link.Target : link.Source;

        if (dependency.StatusCategory?.Name == null)
        {
            throw new InvalidOperationException("Status category is required for dependency");
        }

        var dependencyStatus = DependencyStatusExtensions.FromStatusCategoryString(dependency.StatusCategory.Name);

        return new ScopedDependencyDto
        {
            Id = link.Id,
            Dependency = dependency,
            Type = isOutbound ? "Successor" : "Predecessor",
            Status = SimpleNavigationDto.FromEnum(dependencyStatus),
            CreatedOn = link.CreatedOn,
            CreatedBy = link.CreatedBy,
            Comment = link.Comment
        };
    }
}
