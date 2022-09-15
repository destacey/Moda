using CSharpFunctionalExtensions;

namespace Moda.Work.Domain.Models;

/// <summary>
/// A backlog level helps abstract work types
/// </summary>
public class BacklogLevel : BaseAuditableEntity<Guid>, IAggregateRoot, IActivatable
{
    private BacklogLevel() { }

    public BacklogLevel(string name, string? description, byte order)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Order = order;
    }

    /// <summary>
    /// The name of the backlog level.
    /// </summary>
    public string Name { get; private set; } = null!;

    /// <summary>
    /// The description of the backlog level.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// The order in which the backlog levels are displayed. The lower the number, the higher the level. 
    /// The minimum value is 0 and the maximum value is 255.
    /// </summary>
    public byte Order { get; private set; }

    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// The process for updating the backlog level properties.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public Result Update(string name, string? description, byte order)
    {
        Name = name.Trim();
        Description = description?.Trim();
        Order = order;

        return Result.Success();
    }

    public Result Activate()
    {
        throw new NotImplementedException();
    }

    public Result Deactivate()
    {
        throw new NotImplementedException();
    }
}
