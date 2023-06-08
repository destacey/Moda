using Moda.Common.Extensions;
using Moda.Planning.Application.ProgramIncrements.Dtos;
using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.ProgramIncrements.Queries;
public sealed record GetProgramIncrementObjectiveTypesQuery() : IQuery<IReadOnlyList<ProgramIncrementObjectiveTypeDto>>;

internal sealed class GetProgramIncrementObjectiveTypesQueryHandler : IQueryHandler<GetProgramIncrementObjectiveTypesQuery, IReadOnlyList<ProgramIncrementObjectiveTypeDto>>
{
    public Task<IReadOnlyList<ProgramIncrementObjectiveTypeDto>> Handle(GetProgramIncrementObjectiveTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProgramIncrementObjectiveTypeDto> values = Enum.GetValues<ProgramIncrementObjectiveType>().Select(t => new ProgramIncrementObjectiveTypeDto
        {
            Id = (int)t,
            Name = t.GetDisplayName(),
            Order = t.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
