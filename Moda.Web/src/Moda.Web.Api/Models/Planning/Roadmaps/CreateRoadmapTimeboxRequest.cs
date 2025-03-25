using Moda.Planning.Application.Roadmaps.Commands;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using OneOf;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record CreateRoadmapTimeboxRequest : CreateRoadmapItemRequest
{
    /// <summary>
    /// The Timebox start date.
    /// </summary>
    public LocalDate Start { get; set; }

    /// <summary>
    /// The Timebox end date.
    /// </summary>
    public LocalDate End { get; set; }

    public CreateRoadmapItemCommand ToCreateRoadmapItemCommand()
    {
        return new CreateRoadmapItemCommand(RoadmapId, OneOf<IUpsertRoadmapActivity, IUpsertRoadmapMilestone, IUpsertRoadmapTimebox>.FromT2(new RoadmapTimeboxAdapter(this)));
    }

    private record RoadmapTimeboxAdapter : IUpsertRoadmapTimebox
    {
        private readonly CreateRoadmapTimeboxRequest _request;

        public RoadmapTimeboxAdapter(CreateRoadmapTimeboxRequest request)
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

public sealed class CreateRoadmapTimeboxRequestValidator : CustomValidator<CreateRoadmapTimeboxRequest>
{
    public CreateRoadmapTimeboxRequestValidator()
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
