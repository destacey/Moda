namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed class UpdateRoadmapLinkOrderRequest
{
    public Guid RoadmapId { get; set; }
    public Guid RoadmapLinkId { get; set; }
    public int Order { get; set; }
}

public sealed class UpdateRoadmapLinkOrderRequestValidator : CustomValidator<UpdateRoadmapLinkOrderRequest>
{
    public UpdateRoadmapLinkOrderRequestValidator()
    {
        RuleFor(o => o.RoadmapId)
            .NotEmpty()
            .WithMessage("A valid roadmap must be provided.");

        RuleFor(o => o.RoadmapLinkId)
            .NotEmpty()
            .WithMessage("A valid roadmap link must be provided.");

        RuleFor(o => o.Order)
            .GreaterThan(0)
            .WithMessage("Order must be greater than 0.");
    }
}
