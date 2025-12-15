using Moda.Common.Extensions;
using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Enums;
using TaskStatus = Moda.ProjectPortfolioManagement.Domain.Enums.TaskStatus;

namespace Moda.ProjectPortfolioManagement.Application.ProjectTasks.Queries;

public sealed record GetTaskStatusesQuery : IQuery<IReadOnlyList<TaskStatusDto>> { }

internal sealed class GetTaskStatusesQueryHandler : IQueryHandler<GetTaskStatusesQuery, IReadOnlyList<TaskStatusDto>>
{
    public Task<IReadOnlyList<TaskStatusDto>> Handle(GetTaskStatusesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<TaskStatusDto> values = Enum.GetValues<TaskStatus>().Select(c => new TaskStatusDto
        {
            Id = (int)c,
            Name = c.GetDisplayName(),
            Description = c.GetDisplayDescription(),
            Order = c.GetDisplayOrder()
        }).ToList();

        return Task.FromResult(values);
    }
}
