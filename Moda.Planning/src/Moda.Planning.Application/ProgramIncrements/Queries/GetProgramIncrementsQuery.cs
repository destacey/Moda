using Moda.Planning.Application.ProgramIncrements.Dtos;

namespace Moda.Planning.Application.ProgramIncrements.Queries;
public sealed record GetProgramIncrementsQuery() : IQuery<IReadOnlyList<ProgramIncrementListDto>>;

internal sealed class GetProgramIncrementsQueryHandler : IQueryHandler<GetProgramIncrementsQuery, IReadOnlyList<ProgramIncrementListDto>>
{
    private readonly IPlanningDbContext _planningDbContext;
    private readonly IDateTimeService _dateTimeService;

    public GetProgramIncrementsQueryHandler(IPlanningDbContext planningDbContext, IDateTimeService dateTimeService)
    {
        _planningDbContext = planningDbContext;
        _dateTimeService = dateTimeService;
    }

    public async Task<IReadOnlyList<ProgramIncrementListDto>> Handle(GetProgramIncrementsQuery request, CancellationToken cancellationToken)
    {
        return await _planningDbContext.ProgramIncrements
            .Select(p => ProgramIncrementListDto.Create(p, _dateTimeService))
            .ToListAsync(cancellationToken);
    }
}
