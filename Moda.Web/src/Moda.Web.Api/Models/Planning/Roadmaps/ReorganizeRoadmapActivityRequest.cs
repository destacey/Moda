using Moda.Planning.Application.Roadmaps.Commands;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record ReorganizeRoadmapActivityRequest
{
    public Guid RoadmapId { get; set; }
    public Guid? ParentActivityId { get; set; }
    public Guid ActivityId { get; set; }
    public int Order { get; set; }

    public ReorganizeRoadmapActivityCommand ToReorganizeRoadmapActivityCommand()
    {
        return new ReorganizeRoadmapActivityCommand(RoadmapId, ParentActivityId, ActivityId, Order);
    }
}

public sealed class ReorganizeRoadmapActivityRequestValidator : CustomValidator<ReorganizeRoadmapActivityRequest>
{
    public ReorganizeRoadmapActivityRequestValidator()
    {
        RuleFor(o => o.RoadmapId)
            .NotEmpty()
            .WithMessage("A valid roadmap id must be provided.");

        When(o => o.ParentActivityId.HasValue, () =>
        {
            RuleFor(o => o.ParentActivityId)
                .NotEmpty()
                .WithMessage("A valid parent activity id must be provided.");
        });

        RuleFor(o => o.ActivityId)
            .NotEmpty()
            .WithMessage("A valid activity id must be provided.");

        RuleFor(o => o.Order)
            .GreaterThan(0)
            .WithMessage("Order must be greater than 0.");
    }
}
