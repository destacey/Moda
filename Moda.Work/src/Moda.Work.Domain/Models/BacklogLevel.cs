namespace Moda.Work.Domain.Models;

/// <summary>
/// A backlog level helps abstract work types
/// </summary>
public class BacklogLevel : BaseAuditableEntity<Guid>
{    
    public BacklogLevel(string name, string? description, byte order)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Order = order;
    }

    /// <summary>
    /// The name of the backlog level.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The description of the backlog level.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// The order in which the backlog levels are displayed. The lower the number, the higher the level. 
    /// The minimum value is 0 and the maximum value is 255.
    /// </summary>
    public byte Order { get; }
}
