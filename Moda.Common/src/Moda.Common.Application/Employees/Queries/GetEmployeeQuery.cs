using System.Linq.Expressions;
using Mapster;
using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Application.Persistence;
using Moda.Common.Domain.Employees;

namespace Moda.Common.Application.Employees.Queries;
public sealed record GetEmployeeQuery : IQuery<EmployeeDetailsDto?>
{
    public GetEmployeeQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Employee>();
    }

    public Expression<Func<Employee, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetEmployeeQueryHandler(
    IModaDbContext modaDbContext) 
    : IQueryHandler<GetEmployeeQuery, EmployeeDetailsDto?>
{
    private readonly IModaDbContext _modaDbContext = modaDbContext;

    public async Task<EmployeeDetailsDto?> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        return await _modaDbContext.Employees
            .Where(request.IdOrKeyFilter)
            .ProjectToType<EmployeeDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
