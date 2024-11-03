﻿using CSharpFunctionalExtensions;
using Moda.Planning.Domain.Enums;
using Moda.Planning.Domain.Interfaces.Roadmaps;
using NodaTime;

namespace Moda.Planning.Domain.Models.Roadmaps;
public sealed class RoadmapMilestone : BaseRoadmapItem
{
    private RoadmapMilestone() { }

    private RoadmapMilestone(Guid roadmapId, string name, string? description, LocalDate date, Guid? parentId, string? color)
    {
        RoadmapId = roadmapId;
        Name = name;
        Description = description;
        Type = RoadmapItemType.Milestone;
        Date = date;
        ParentId = parentId;
        Color = color;
    }

    public LocalDate Date { get; private set; }

    /// <summary>
    /// Updates the Roadmap Milestone.
    /// </summary>
    /// <param name="roadmapMilestone"></param>
    /// <returns></returns>
    internal Result Update(IUpsertRoadmapMilestone roadmapMilestone, RoadmapActivity? parentActivity)
    {
        // TODO: this initial implementation requires going through the Roadmap to update the Roadmap Milestone. This is needed to verify permissions against the Roadmap within the Domain layer.

        if (ParentId != roadmapMilestone.ParentId)
        {
            var changeParentResult = ChangeParent(parentActivity);
            if (changeParentResult.IsFailure)
            {
                return changeParentResult;
            }
        }

        Name = roadmapMilestone.Name;
        Description = roadmapMilestone.Description;
        Date = roadmapMilestone.Date;
        Color = roadmapMilestone.Color;

        return Result.Success();
    }

    /// <summary>
    /// Creates a new Roadmap RoadmapMilestone.
    /// </summary>
    /// <param name="roadmapId"></param>
    /// <param name="parentId"></param>
    /// <param name="milestone"></param>
    /// <returns></returns>
    internal static RoadmapMilestone Create(Guid roadmapId, Guid? parentId, IUpsertRoadmapMilestone milestone)
    {
        return new RoadmapMilestone(roadmapId, milestone.Name, milestone.Description, milestone.Date, parentId, milestone.Color);
    }
}
