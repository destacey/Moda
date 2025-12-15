namespace Moda.Web.Api.Models.Ppm.ProjectTasks;

public sealed record UpdateProjectTaskOrderRequest
{
    /// <summary>
    /// The ID of the task.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// The new order/position of the task within its parent.
    /// </summary>
    public int Order { get; set; }
}

public sealed class UpdateProjectTaskOrderRequestValidator : CustomValidator<UpdateProjectTaskOrderRequest>
{
    public UpdateProjectTaskOrderRequestValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty();

        RuleFor(x => x.Order)
            .GreaterThan(0)
            .WithMessage("Order must be greater than 0.");
    }
}
