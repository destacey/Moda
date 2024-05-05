﻿using Moda.Common.Application.Dtos;
using Moda.Goals.Application.Objectives.Dtos;
using Moda.Planning.Application.Models;

namespace Moda.Planning.Application.PlanningIntervals.Dtos;
public sealed record PlanningIntervalObjectiveListDto
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the objective.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the status.</summary>
    /// <value>The status.</value>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>Gets or sets the health status.</summary>
    /// <value>The status.</value>
    public PlanningHealthCheckDto? HealthCheck { get; set; }

    public required NavigationDto PlanningInterval { get; set; }

    public required PlanningTeamNavigationDto Team { get; set; }

    public double Progress { get; set; }

    /// <summary>Gets or sets the type.</summary>
    /// <value>The PI objective type.</value>
    public required SimpleNavigationDto Type { get; set; }

    /// <summary>Gets or sets the start date.</summary>
    /// <value>The start date.</value>
    public LocalDate? StartDate { get; set; }

    /// <summary>Gets or sets the target date.</summary>
    /// <value>The target date.</value>
    public LocalDate? TargetDate { get; set; }

    /// <summary>Gets a value indicating whether this instance is stretch.</summary>
    /// <value><c>true</c> if this instance is stretch; otherwise, <c>false</c>.</value>
    public bool IsStretch { get; set; }

    /// <summary>
    /// The order of the Objective compared to other Objectives in the same context.
    /// </summary>
    public int? Order { get; set; }

    public static PlanningIntervalObjectiveListDto Create(PlanningIntervalObjective piObjective, ObjectiveListDto objective, NavigationDto piNavigationDto, Instant now)
    {
        return new PlanningIntervalObjectiveListDto()
        {
            Id = piObjective.Id,
            Key = piObjective.Key,
            Name = objective.Name,
            PlanningInterval = piNavigationDto,
            Status = SimpleNavigationDto.FromEnum(piObjective.Status),
            HealthCheck = piObjective.HealthCheck is not null ? PlanningHealthCheckDto.Create(piObjective.HealthCheck, now) : null,
            Progress = objective.Progress,
            Team = PlanningTeamNavigationDto.FromPlanningTeam(piObjective.Team),
            Type = SimpleNavigationDto.FromEnum(piObjective.Type),
            StartDate = objective.StartDate,
            TargetDate = objective.TargetDate,
            IsStretch = piObjective.IsStretch,
            Order = objective.Order
        };
    }
}
