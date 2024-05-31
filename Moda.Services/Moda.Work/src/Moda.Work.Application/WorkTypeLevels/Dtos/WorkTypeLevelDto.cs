using Moda.Common.Domain.Enums.Work;

namespace Moda.Work.Application.WorkTypeLevels.Dtos;
public sealed record WorkTypeLevelDto : IMapFrom<WorkTypeLevel>
{
    public int Id { get; set; }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>Gets or sets the tier.</summary>
    /// <value>The tier.</value>
    public WorkTypeTier Tier { get; set; }

    /// <summary>Gets or sets the order.</summary>
    /// <value>The order.</value>
    public int Order { get; set; }
}
