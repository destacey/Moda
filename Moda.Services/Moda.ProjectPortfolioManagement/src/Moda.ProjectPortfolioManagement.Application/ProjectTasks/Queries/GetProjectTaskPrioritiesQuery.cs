using Moda.Common.Extensions;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Queries;

public sealed record GetProjectTaskPrioritiesQuery : IQuery<IReadOnlyList<TaskPriorityDto>> { }

internal sealed class GetProjectTaskPrioritiesQueryHandler : IQueryHandler<GetProjectTaskPrioritiesQuery, IReadOnlyList<TaskPriorityDto>>
{
    public Task<IReadOnlyList<TaskPriorityDto>> Handle(GetProjectTaskPrioritiesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<TaskPriorityDto> values = [.. Enum.GetValues<TaskPriority>().Select(c => new TaskPriorityDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        })];

        return Task.FromResult(values);
    }
}
