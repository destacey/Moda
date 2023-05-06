using Moda.Planning.Application.ProgramIncrements.Dtos;

namespace Moda.Planning.Application.ProgramIncrements.Queries;
public sealed record GetProgramIncrementsQuery() : IQuery<IReadOnlyList<ProgramIncrementListDto>>;

internal sealed class GetProgramIncrementsQueryHandler : IQueryHandler<GetProgramIncrementsQuery, IReadOnlyList<ProgramIncrementListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;

    public GetProgramIncrementsQueryHandler(IPlanningDbContext planningDbContext)
    {
        _planningDbContext = planningDbContext;
    }

    public async Task<IReadOnlyList<ProgramIncrementListDto>> Handle(GetProgramIncrementsQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.ProgramIncrements
            .ProjectToType<ProgramIncrementListDto>()
            .ToListAsync(cancellationToken);
    }
}
