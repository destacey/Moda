using Ardalis.GuardClauses;
using Moda.Common.Extensions;
using Moda.Organization.Domain.Enums;
using Moda.Organization.Domain.Interfaces;

namespace Moda.Organization.Domain.Models;
public abstract class BaseTeam : BaseAuditableEntity<Guid>
{
    private string _name = null!;
    private TeamCode _code = null!;
    private string? _description;

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; protected set; }

    /// <summary>
    /// The name of the workspace.
    /// </summary>
    public string Name
    {
        get => _name;
        protected set => _name = Guard.Against.NullOrWhiteSpace(value, nameof(Name)).Trim();
    }

    /// <summary>Gets the code.</summary>
    /// <value>The code.</value>
    public TeamCode Code
    {
        get => _code;
        protected set => _code = Guard.Against.Null(value, nameof(Code));
    }

    /// <summary>
    /// The description of the workspace.
    /// </summary>
    public string? Description
    {
        get => _description;
        protected set => _description = value.NullIfWhiteSpacePlusTrim();
    }

    /// <summary>Gets the type.  This value should not change.</summary>
    /// <value>The type.</value>
    public TeamType Type { get; protected set; }

    /// <summary>
    /// Indicates whether the organization is active or not.  
    /// </summary>
    public bool IsActive { get; protected set; } = true;
}
