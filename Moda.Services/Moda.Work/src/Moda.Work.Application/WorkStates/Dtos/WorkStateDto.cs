namespace Moda.Work.Application.WorkStates.Dtos;
public sealed record WorkStateDto : IMapFrom<WorkState>
{
    public int Id { get; set; }

    /// <summary>The name of the work state.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work state.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>Indicates whether the work state is active or not.</summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; set; }
}
