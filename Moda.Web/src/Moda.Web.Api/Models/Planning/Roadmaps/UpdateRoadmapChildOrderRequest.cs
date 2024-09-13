namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed class UpdateRoadmapChildOrderRequest
{
    public Guid RoadmapId { get; set; }
    public Guid ChildRoadmapId { get; set; }
    public int Order { get; set; }
}

public sealed class UpdateRoadmapChildOrderRequestValidator : CustomValidator<UpdateRoadmapChildOrderRequest>
{
    public UpdateRoadmapChildOrderRequestValidator()
    {
        RuleFor(o => o.RoadmapId)
            .NotEmpty()
            .WithMessage("A valid roadmap id must be provided.");

        RuleFor(o => o.ChildRoadmapId)
            .NotEmpty()
            .WithMessage("A valid child roadmap id must be provided.");

        RuleFor(o => o.Order)
            .GreaterThan(0)
            .WithMessage("Order must be greater than 0.");
    }
}
