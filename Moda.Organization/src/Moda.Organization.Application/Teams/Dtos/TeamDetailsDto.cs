namespace Moda.Organization.Application.Teams.Dtos;
public class TeamDetailsDto : IMapFrom<Team>
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; set; }

    /// <summary>
    /// The name of the workspace.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public required string Code { get; set; }

    /// <summary>
    /// The description of the workspace.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>Gets the team type.</summary>
    /// <value>The team type.</value>
    public required string Type { get; set; }

    /// <summary>
    /// Indicates whether the organization is active or not.  
    /// </summary>
    public bool IsActive { get; set; }
}
