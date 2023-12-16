using Mapster;
using Moda.Common.Domain.Enums;

namespace Moda.Work.Application.BacklogLevels.Dtos;
public sealed record BacklogLevelDto : IMapFrom<BacklogLevel>
{
    public int Id { get; set; }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>Gets or sets the category.</summary>
    /// <value>The category.</value>
    public BacklogCategory Category { get; set; }

    /// <summary>Gets or sets the rank. The higher the rank, the higher the priority.</summary>
    /// <value>The rank.</value>
    public int Rank { get; set; }
}
