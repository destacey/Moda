using Mapster;
using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Application.Persistence;

namespace Moda.Common.Application.Employees.Queries;
public sealed record GetDirectReportsQuery(Guid EmployeeId) : IQuery<IReadOnlyList<EmployeeListDto>>;

internal sealed class GetDirectReportsQueryHandler : IQueryHandler<GetDirectReportsQuery, IReadOnlyList<EmployeeListDto>>
{
    private readonly IModaDbContext _modaDbContext;
    private readonly ILogger<GetDirectReportsQueryHandler> _logger;

    public GetDirectReportsQueryHandler(IModaDbContext modaDbContext, ILogger<GetDirectReportsQueryHandler> logger)
    {
        _modaDbContext = modaDbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<EmployeeListDto>> Handle(GetDirectReportsQuery request, CancellationToken cancellationToken)
    {
        return await _modaDbContext.Employees
            .Where(e => e.ManagerId == request.EmployeeId && e.IsActive)
            .ProjectToType<EmployeeListDto>()
            .ToListAsync(cancellationToken);
    }
}
