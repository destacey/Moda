using Moda.Common.Domain.Enums.Planning;
using Moda.Planning.Application.Iterations.Dtos;

namespace Moda.Planning.Application.Iterations.Queries;

public sealed record GetTeamActiveSprintQuery(Guid TeamId) : IQuery<SprintDetailsDto?>;

public sealed class GetTeamActiveSprintQueryValidator : CustomValidator<GetTeamActiveSprintQuery>
{
    public GetTeamActiveSprintQueryValidator()
    {
        RuleFor(q => q.TeamId)
            .NotEmpty();
    }
}

internal sealed class GetTeamActiveSprintQueryHandler(IPlanningDbContext planningDbContext)
    : IQueryHandler<GetTeamActiveSprintQuery, SprintDetailsDto?>
{
    private readonly IPlanningDbContext _planningDbContext = planningDbContext;

    public async Task<SprintDetailsDto?> Handle(GetTeamActiveSprintQuery request, CancellationToken cancellationToken)
    {
        var sprint = await _planningDbContext.Iterations
            .Where(i => i.TeamId == request.TeamId && i.Type == IterationType.Sprint)
            .ProjectToType<SprintDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return sprint;
    }
}
