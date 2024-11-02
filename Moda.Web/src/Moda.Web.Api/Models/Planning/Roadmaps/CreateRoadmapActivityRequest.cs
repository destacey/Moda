using Moda.Planning.Application.Roadmaps.Commands;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using OneOf;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record CreateRoadmapActivityRequest : CreateRoadmapItemRequest
{
    /// <summary>
    /// The Activity start date.
    /// </summary>
    public required LocalDate Start { get; set; }

    /// <summary>
    /// The Activity end date.
    /// </summary>
    public required LocalDate End { get; set; }

    public CreateRoadmapItemCommand ToCreateRoadmapItemCommand()
    {
        return new CreateRoadmapItemCommand(RoadmapId, OneOf<IUpsertRoadmapActivity, IUpsertRoadmapMilestone, IUpsertRoadmapTimebox>.FromT0(new RoadmapActivityAdapter(this)));
    }

    private record RoadmapActivityAdapter : IUpsertRoadmapActivity
    {
        private readonly CreateRoadmapActivityRequest _request;

        public RoadmapActivityAdapter(CreateRoadmapActivityRequest request)
        {
            _request = request;
        }

        public string Name => _request.Name;
        public string? Description => _request.Description;
        public Guid? ParentId => _request.ParentId;
        public string? Color => _request.Color;
        public LocalDateRange DateRange => new(_request.Start, _request.End);
    }
}

public sealed class CreateRoadmapActivityRequestValidator : CustomValidator<CreateRoadmapActivityRequest>
{
    public CreateRoadmapActivityRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new CreateRoadmapItemRequestValidator());

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .NotNull()
            .Must((membership, end) => membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
