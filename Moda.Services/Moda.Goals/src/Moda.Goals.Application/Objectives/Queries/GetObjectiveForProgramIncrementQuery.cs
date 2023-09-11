using Moda.Goals.Application.Objectives.Dtos;
using Moda.Goals.Application.Persistence;

namespace Moda.Goals.Application.Objectives.Queries;
public sealed record GetObjectiveForProgramIncrementQuery(Guid Id, Guid ProgramIncrementId) : IQuery<ObjectiveDetailsDto?>;

internal sealed class GetObjectiveForProgramIncrementQueryHandler : IQueryHandler<GetObjectiveForProgramIncrementQuery, ObjectiveDetailsDto?>
{
    private readonly IGoalsDbContext _goalsDbContext;

    public GetObjectiveForProgramIncrementQueryHandler(IGoalsDbContext goalsDbContext)
    {
        _goalsDbContext = goalsDbContext;
    }

    public async Task<ObjectiveDetailsDto?> Handle(GetObjectiveForProgramIncrementQuery request, CancellationToken cancellationToken)
    {
        return await _goalsDbContext.Objectives
            .Where(o => o.Id == request.Id && o.PlanId == request.ProgramIncrementId)
            .ProjectToType<ObjectiveDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
