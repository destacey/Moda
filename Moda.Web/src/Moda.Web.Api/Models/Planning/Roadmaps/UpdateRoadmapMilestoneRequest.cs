using Moda.Planning.Application.Roadmaps.Commands;
using Moda.Planning.Application.Roadmaps.Dtos;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using OneOf;

namespace Moda.Web.Api.Models.Planning.Roadmaps;

public sealed record UpdateRoadmapMilestoneRequest : UpdateRoadmapItemRequest
{
    /// <summary>
    /// The Milestone date.
    /// </summary>
    public LocalDate Date { get; set; }

    /// <summary>
    /// Creates an UpdateRoadmapMilestoneRequest from a RoadmapMilestoneDetailsDto.
    /// </summary>
    public static UpdateRoadmapMilestoneRequest FromDto(RoadmapMilestoneDetailsDto dto)
    {
        return new UpdateRoadmapMilestoneRequest
        {
            RoadmapId = dto.RoadmapId,
            ItemId = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            ParentId = dto.Parent?.Id,
            Color = dto.Color,
            Date = dto.Date
        };
    }

    public UpdateRoadmapItemCommand ToUpdateRoadmapItemCommand()
    {
        return new UpdateRoadmapItemCommand(RoadmapId, ItemId, OneOf<IUpsertRoadmapActivity, IUpsertRoadmapMilestone, IUpsertRoadmapTimebox>.FromT1(new UpsertRoadmapMilestoneAdapter(this)));
    }
}

public sealed class UpdateRoadmapMilestoneRequestValidator : CustomValidator<UpdateRoadmapMilestoneRequest>
{
    public UpdateRoadmapMilestoneRequestValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        Include(new UpdateRoadmapItemRequestValidator());

        RuleFor(t => t.Date)
            .NotNull();
    }
}
