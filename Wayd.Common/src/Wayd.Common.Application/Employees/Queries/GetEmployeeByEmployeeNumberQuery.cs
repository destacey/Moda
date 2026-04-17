using Wayd.Common.Application.Persistence;

namespace Wayd.Common.Application.Employees.Queries;

public sealed record GetEmployeeByEmployeeNumberQuery(string EmployeeNumber) : IQuery<Guid?>;

internal sealed class GetEmployeeByEmployeeNumberQueryHandler : IQueryHandler<GetEmployeeByEmployeeNumberQuery, Guid?>
{
    private readonly IWaydDbContext _modaDbContext;

    public GetEmployeeByEmployeeNumberQueryHandler(IWaydDbContext modaDbContext)
    {
        _modaDbContext = modaDbContext;
    }

    public async Task<Guid?> Handle(GetEmployeeByEmployeeNumberQuery request, CancellationToken cancellationToken)
    {
        return await _modaDbContext.Employees
            .Where(e => e.EmployeeNumber == request.EmployeeNumber)
            .Select(e => e.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
