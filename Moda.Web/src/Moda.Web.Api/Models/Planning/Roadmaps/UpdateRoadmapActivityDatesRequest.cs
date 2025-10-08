using Moda.Planning.Application.Roadmaps.Commands;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using OneOf;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record UpdateRoadmapActivityDatesRequest : UpdateRoadmapItemDatesRequest
{
    /// <summary>
    /// The Roadmap Item start date.
    /// </summary>
    public LocalDate Start { get; set; }

    /// <summary>
    /// The Roadmap Item end date.
    /// </summary>
    public LocalDate End { get; set; }

    public UpdateRoadmapItemDatesCommand ToUpdateRoadmapItemDatesCommand()
    {
        var dateUpdate = OneOf<IUpsertRoadmapActivityDateRange, IUpsertRoadmapMilestoneDate, IUpsertRoadmapTimeboxDateRange>.FromT0(new UpsertRoadmapActivityDateRangeAdapter(this));

        return new UpdateRoadmapItemDatesCommand(RoadmapId, ItemId, dateUpdate);
    }
}

public sealed class UpdateRoadmapActivityDatesRequestValidator : CustomValidator<UpdateRoadmapActivityDatesRequest>
{
    public UpdateRoadmapActivityDatesRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(t => t.Start)
            .NotNull();

        RuleFor(t => t.End)
            .NotNull()
            .Must((membership, end) => membership.Start <= end)
                .WithMessage("End date must be greater than or equal to start date");
    }
}
