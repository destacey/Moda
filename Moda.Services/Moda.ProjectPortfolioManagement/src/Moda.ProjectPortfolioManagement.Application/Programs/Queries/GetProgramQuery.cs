using System.Linq.Expressions;
using Moda.Common.Application.Models;
using Moda.ProjectPortfolioManagement.Application.Programs.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models;

namespace Moda.ProjectPortfolioManagement.Application.Programs.Queries;

public sealed record GetProgramQuery : IQuery<ProgramDetailsDto?>
{
    public GetProgramQuery(IdOrKey idOrKey)
    {
        IdOrKeyFilter = idOrKey.CreateFilter<Program>();
    }

    public Expression<Func<Program, bool>> IdOrKeyFilter { get; }
}

internal sealed class GetProgramQueryHandler(IProjectPortfolioManagementDbContext ppmDbContext)
    : IQueryHandler<GetProgramQuery, ProgramDetailsDto?>
{
    private readonly IProjectPortfolioManagementDbContext _ppmDbContext = ppmDbContext;

    public async Task<ProgramDetailsDto?> Handle(GetProgramQuery request, CancellationToken cancellationToken)
    {
        return await _ppmDbContext.Programs
            .Where(request.IdOrKeyFilter)
            .ProjectToType<ProgramDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}