using Microsoft.EntityFrameworkCore;

namespace Moda.Organization.Application.Employees.Queries;

public sealed record GetEmployeeByEmployeeNumberQuery(string EmployeeNumber) : IQuery<Guid?>;

internal sealed class GetEmployeeByEmployeeNumberQueryHandler : IQueryHandler<GetEmployeeByEmployeeNumberQuery, Guid?>
{
    private readonly IOrganizationDbContext _organizationDbContext;

    public GetEmployeeByEmployeeNumberQueryHandler(IOrganizationDbContext organizationDbContext)
    {
        _organizationDbContext = organizationDbContext;
    }

    public async Task<Guid?> Handle(GetEmployeeByEmployeeNumberQuery request, CancellationToken cancellationToken)
    {
        return await _organizationDbContext.Employees
            .Where(e => e.EmployeeNumber == request.EmployeeNumber)
            .Select(e => e.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
