using Moda.Common.Extensions;
using Moda.Goals.Domain.Enums;
using Moda.Planning.Application.ProgramIncrements.Dtos;

namespace Moda.Planning.Application.ProgramIncrements.Queries;

public sealed record GetProgramIncrementObjectiveStatusesQuery : IQuery<IReadOnlyList<ProgramIncrementObjectiveStatusDto>> { }

internal sealed class GetProgramIncrementObjectiveStatusesQueryHandler : IQueryHandler<GetProgramIncrementObjectiveStatusesQuery, IReadOnlyList<ProgramIncrementObjectiveStatusDto>>
{
    public Task<IReadOnlyList<ProgramIncrementObjectiveStatusDto>> Handle(GetProgramIncrementObjectiveStatusesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProgramIncrementObjectiveStatusDto> values = Enum.GetValues<ObjectiveStatus>().Select(c => new ProgramIncrementObjectiveStatusDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
