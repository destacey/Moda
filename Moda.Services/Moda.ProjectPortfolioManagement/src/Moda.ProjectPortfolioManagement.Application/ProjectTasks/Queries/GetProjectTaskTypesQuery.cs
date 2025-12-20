using Moda.Common.Extensions;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Queries;

public sealed record GetProjectTaskTypesQuery : IQuery<IReadOnlyList<ProjectTaskTypeDto>> { }

internal sealed class GetProjectTaskTypesQueryHandler : IQueryHandler<GetProjectTaskTypesQuery, IReadOnlyList<ProjectTaskTypeDto>>
{
    public Task<IReadOnlyList<ProjectTaskTypeDto>> Handle(GetProjectTaskTypesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProjectTaskTypeDto> values = Enum.GetValues<ProjectTaskType>().Select(c => new ProjectTaskTypeDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
