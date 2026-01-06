using Moda.ProjectPortfolioManagement.Application.ProjectTasks.Commands;

namespace Moda.Web.Api.Models.Ppm.ProjectTasks;

public sealed record UpdateProjectTaskPlacementRequest
{
    /// <summary>
    /// The ID of the task.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// The ID of the new parent task. If null, the task will be moved to the root level.
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// The new order/position of the task within its parent.
    /// </summary>
    public int? Order { get; set; }

    public UpdateProjectTaskPlacementCommand ToUpdateProjectTaskPlacementCommand(Guid projectId)
        => new(projectId, TaskId, ParentId, Order);
}

public sealed class UpdateProjectTaskPlacementRequestValidator : CustomValidator<UpdateProjectTaskPlacementRequest>
{
    public UpdateProjectTaskPlacementRequestValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.ParentId)
            .Must(id => id == null || id != Guid.Empty)
            .WithMessage("ParentId cannot be an empty GUID.");

        RuleFor(x => x.Order)
            .GreaterThan(0)
                .When(x => x.Order.HasValue)
                .WithMessage("Order must be greater than 0 when specified.");
    }
}
