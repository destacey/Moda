﻿using Moda.Common.Application.Dtos;
using Moda.Goals.Domain.Models;

namespace Moda.Goals.Application.Objectives.Dtos;
public sealed record ObjectiveListDto : IMapFrom<Objective>
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

    /// <summary>Gets or sets the type.</summary>
    /// <value>The PI objective type.</value>
    public required SimpleNavigationDto Type { get; set; }

    /// <summary>Gets or sets the status.</summary>
    /// <value>The status.</value>
    public required SimpleNavigationDto Status { get; set; }

    /// <summary>Gets or sets the progress.</summary>
    /// <value>The progress.</value>
    public double Progress { get; set; }

    /// <summary>Gets or sets the owner identifier.</summary>
    /// <value>The owner identifier.</value>
    public Guid? OwnerId { get; set; }

    /// <summary>Gets or sets the plan identifier.</summary>
    /// <value>The plan identifier.</value>
    public Guid? PlanId { get; set; }

    /// <summary>Gets or sets the start date.</summary>
    /// <value>The start date.</value>
    public LocalDate? StartDate { get; set; }

    /// <summary>Gets or sets the target date.</summary>
    /// <value>The target date.</value>
    public LocalDate? TargetDate { get; set; }

    /// <summary>
    /// The order of the Objective compared to other Objectives in the same context.
    /// </summary>
    public int? Order { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Objective, ObjectiveListDto>()
            .Map(dest => dest.Status, src => SimpleNavigationDto.FromEnum(src.Status))
            .Map(dest => dest.Type, src => SimpleNavigationDto.FromEnum(src.Type));
    }
}
