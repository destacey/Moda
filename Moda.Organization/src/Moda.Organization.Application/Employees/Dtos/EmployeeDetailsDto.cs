using Mapster;
using Moda.Common.Helpers;
using NodaTime;

namespace Moda.Organization.Application.Employees.Dtos;
public sealed record EmployeeDetailsDto : IMapFrom<Employee>
{
    /// <summary>Gets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int LocalId { get; set; }

    /// <summary>Gets the first name.</summary>
    /// <value>The first name.</value>
    public required string FirstName { get; set; }

    /// <summary>Gets the middle name.</summary>
    /// <value>The middle name.</value>
    public string? MiddleName { get; set; }

    /// <summary>Gets the last name.</summary>
    /// <value>The last name.</value>
    public required string LastName { get; set; }

    /// <summary>Gets the suffix.</summary>
    /// <value>The suffix.</value>
    public string? Suffix { get; set; }

    /// <summary>Gets the employee's personal title.</summary>
    /// <value>The title.</value>
    public string? Title { get; set; }

    /// <summary>Gets the employee number.</summary>
    /// <value>The employee number.</value>
    public required string EmployeeNumber { get; set; }

    /// <summary>Gets the hire date.</summary>
    /// <value>The hire date.</value>
    public Instant? HireDate { get; set; }

    /// <summary>Gets the email.</summary>
    /// <value>The email.</value>
    public required string Email { get; set; }

    /// <summary>Gets the job title.</summary>
    /// <value>The job title.</value>
    public string? JobTitle { get; set; }

    /// <summary>Gets the department.</summary>
    /// <value>The department.</value>
    public string? Department { get; set; }

    /// <summary>Gets the office location.</summary>
    /// <value>The office location.</value>
    public string? OfficeLocation { get; set; }

    /// <summary>Gets the manager identifier.</summary>
    /// <value>The manager identifier.</value>
    public Guid? ManagerId { get; set; }

    /// <summary>Gets the local identifier.</summary>
    /// <value>The local identifier.</value>
    public int? ManagerLocalId { get; set; }

    /// <summary>Gets the manager.</summary>
    /// <value>The manager.</value>
    public string? ManagerName { get; set; }

    /// <summary>
    /// Indicates whether the employee is active or not.  
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// A single string to present the employee's full name
    /// </summary>
    public string? FullName { get; set; }

    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Employee, EmployeeDetailsDto>()
            .Map(dest => dest.FirstName, src => src.Name.FirstName)
            .Map(dest => dest.MiddleName, src => src.Name.MiddleName)
            .Map(dest => dest.LastName, src => src.Name.LastName)
            .Map(dest => dest.Suffix, src => src.Name.Suffix)
            .Map(dest => dest.Title, src => src.Name.Title)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.ManagerLocalId, src => src.Manager!.LocalId)
            .Map(dest => dest.FullName, src => $"{StringHelpers.Concat(src.Name.Title, src.Name.FirstName, src.Name.MiddleName, src.Name.LastName, src.Name.Suffix)}")
            .Map(dest => dest.ManagerName, src => $"{src.Manager!.Name.FirstName} {src.Manager!.Name.LastName}", srcCond => srcCond.ManagerId.HasValue);
    }
}
