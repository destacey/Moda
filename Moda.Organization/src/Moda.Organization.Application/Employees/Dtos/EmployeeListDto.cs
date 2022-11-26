using Mapster;

namespace Moda.Organization.Application.Employees.Dtos;
public sealed record EmployeeListDto : IMapFrom<Employee>
{
    /// <summary>Gets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; private set; }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; private set; }

    /// <summary>Gets the first name.</summary>
    /// <value>The first name.</value>
    public string FirstName { get; private set; } = null!;

    /// <summary>Gets the middle name.</summary>
    /// <value>The middle name.</value>
    public string? MiddleName { get; private set; }

    /// <summary>Gets the last name.</summary>
    /// <value>The last name.</value>
    public string LastName { get; private set; } = null!;

    /// <summary>Gets the suffix.</summary>
    /// <value>The suffix.</value>
    public string? Suffix { get; private set; }

    /// <summary>Gets the employee's personal title.</summary>
    /// <value>The title.</value>
    public string? Title { get; private set; }

    /// <summary>Gets the employee number.</summary>
    /// <value>The employee number.</value>
    public string EmployeeNumber { get; private set; } = null!;

    /// <summary>Gets the email.</summary>
    /// <value>The email.</value>
    public string Email { get; private set; } = null!;

    /// <summary>Gets the job title.</summary>
    /// <value>The job title.</value>
    public string? JobTitle { get; private set; }

    /// <summary>Gets the department.</summary>
    /// <value>The department.</value>
    public string? Department { get; private set; }

    /// <summary>Gets the manager identifier.</summary>
    /// <value>The manager identifier.</value>
    public Guid? ManagerId { get; private set; }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int? ManagerLocalId { get; private set; }

    /// <summary>Gets the manager.</summary>
    /// <value>The manager.</value>
    public string? ManagerName { get; private set; }

    /// <summary>
    /// Indicates whether the employee is active or not.  
    /// </summary>
    public bool IsActive { get; private set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Employee, EmployeeListDto>()
            .Map(dest => dest.FirstName, src => src.Name.FirstName)
            .Map(dest => dest.MiddleName, src => src.Name.MiddleName)
            .Map(dest => dest.LastName, src => src.Name.LastName)
            .Map(dest => dest.Suffix, src => src.Name.Suffix)
            .Map(dest => dest.Title, src => src.Name.Title)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.ManagerLocalId, src => src.Manager!.LocalId)
            .Map(dest => dest.ManagerName, src => $"{src.Manager!.Name.FirstName} {src.Manager!.Name.LastName}", srcCond => srcCond.ManagerId.HasValue);
    }
}
