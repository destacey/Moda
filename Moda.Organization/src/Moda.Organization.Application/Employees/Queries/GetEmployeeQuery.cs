using Mapster;
using Microsoft.EntityFrameworkCore;
using Moda.Organization.Application.Employees.Dtos;

namespace Moda.Organization.Application.Employees.Queries;
public sealed record GetEmployeeQuery : IQuery<EmployeeDetailsDto?>
{
    public GetEmployeeQuery(Guid employeeId)
    {
        EmployeeId = employeeId;
    }

    public Guid EmployeeId { get; }
}

internal sealed class GetEmployeeQueryHandler : IQueryHandler<GetEmployeeQuery, EmployeeDetailsDto?>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public GetEmployeeQueryHandler(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;
    }

    public async Task<EmployeeDetailsDto?> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.Employees
            .ProjectToType<EmployeeDetailsDto>()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
    }
}
