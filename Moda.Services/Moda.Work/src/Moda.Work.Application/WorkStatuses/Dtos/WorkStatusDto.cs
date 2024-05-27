using Moda.Common.Application.Requests.WorkManagement.Interfaces;

namespace Moda.Work.Application.WorkStatuses.Dtos;
public sealed record WorkStatusDto : IMapFrom<WorkStatus>, IWorkStatusDto
{
    public int Id { get; set; }

    /// <summary>The name of the work status.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work status.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>Indicates whether the work status is active or not.</summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; set; }
}
