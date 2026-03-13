using Moda.Common.Extensions;
using Moda.ProjectPortfolioManagement.Application.Programs.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.Programs.Queries;

public sealed record GetProgramStatusesQuery : IQuery<IReadOnlyList<ProgramStatusDto>> { }

internal sealed class GetProgramStatusesQueryHandler : IQueryHandler<GetProgramStatusesQuery, IReadOnlyList<ProgramStatusDto>>
{
    public Task<IReadOnlyList<ProgramStatusDto>> Handle(GetProgramStatusesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProgramStatusDto> values = [.. Enum.GetValues<ProgramStatus>().Select(c => new ProgramStatusDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder(),
            LifecyclePhase = c.GetDisplayGroupName() ?? "Unknown"
        })];

        return Task.FromResult(values);
    }
}
