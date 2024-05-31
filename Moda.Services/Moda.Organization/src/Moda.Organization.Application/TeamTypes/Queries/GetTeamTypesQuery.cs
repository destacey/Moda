using Moda.Common.Domain.Enums.Organization;
using Moda.Organization.Application.TeamTypes.Dtos;

namespace Moda.Organization.Application.TeamTypes.Queries;

public sealed record GetTeamTypesQuery : IQuery<IReadOnlyList<TeamTypeDto>> { }

internal sealed class GetTeamTypesQueryHandler : IQueryHandler<GetTeamTypesQuery, IReadOnlyList<TeamTypeDto>>
{
    public Task<IReadOnlyList<TeamTypeDto>> Handle(GetTeamTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<TeamTypeDto> values = Enum.GetValues<TeamType>().Select(t => new TeamTypeDto
        {
            Id = (int)t,
            Name = t.GetDisplayName(),
            Description = t.GetDisplayDescription(),
            Order = t.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
