using Moda.Common.Extensions;
using Moda.ProjectPortfolioManagement.Application.Portfolios.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.Portfolios.Queries;

public sealed record GetProjectPortfolioStatusesQuery : IQuery<IReadOnlyList<ProjectPortfolioStatusDto>> { }

internal sealed class GetProjectPortfolioStatusesQueryHandler : IQueryHandler<GetProjectPortfolioStatusesQuery, IReadOnlyList<ProjectPortfolioStatusDto>>
{
    public Task<IReadOnlyList<ProjectPortfolioStatusDto>> Handle(GetProjectPortfolioStatusesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProjectPortfolioStatusDto> values = [.. Enum.GetValues<ProjectPortfolioStatus>().Select(c => new ProjectPortfolioStatusDto
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
