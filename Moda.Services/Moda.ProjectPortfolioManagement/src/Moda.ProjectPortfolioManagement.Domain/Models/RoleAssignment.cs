using Wayd.Common.Domain.Employees;

namespace Wayd.ProjectPortfolioManagement.Domain.Models;

public sealed class RoleAssignment<T> : ISystemAuditable where T : Enum
{
    private RoleAssignment() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleAssignment{T}"/> class.
    /// </summary>
    /// <param name="objectId"></param>
    /// <param name="role"></param>
    /// <param name="employeeId"></param>
    internal RoleAssignment(Guid objectId, T role, Guid employeeId)
    {
        ObjectId = objectId;
        Role = role;
        EmployeeId = employeeId;
    }

    public Guid ObjectId { get; private set; } // PortfolioId, ProgramId, or ProjectId

    public T Role { get; private set; } = default!;

    public Guid EmployeeId { get; private set; }

    public Employee? Employee { get; set; }
}
