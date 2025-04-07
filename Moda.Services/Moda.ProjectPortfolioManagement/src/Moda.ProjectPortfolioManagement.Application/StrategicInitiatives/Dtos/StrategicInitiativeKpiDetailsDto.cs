﻿using Moda.Common.Application.Dtos;
using Moda.ProjectPortfolioManagement.Domain.Models.StrategicInitiatives;

namespace Moda.ProjectPortfolioManagement.Application.StrategicInitiatives.Dtos;
public sealed record StrategicInitiativeKpiDetailsDto : IMapFrom<StrategicInitiativeKpi>
{
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the KPI.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the KPI.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description detailing what the KPI measures.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The target value that defines success for the KPI.
    /// </summary>
    public double TargetValue { get; set; }

    /// <summary>
    /// The actual value of the KPI.
    /// </summary>
    public double? ActualValue { get; set; }

    /// <summary>
    /// Gets the unit of measurement for the KPI.
    /// </summary>
    public required SimpleNavigationDto Unit { get; set; }

    /// <summary>
    /// Gets the target direction for the KPI. This indicates whether the KPI is expected to increase or decrease.
    /// </summary>
    public required SimpleNavigationDto TargetDirection { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<StrategicInitiativeKpi, StrategicInitiativeKpiDetailsDto>()
            .Map(dest => dest.Unit, src => SimpleNavigationDto.FromEnum(src.Unit))
            .Map(dest => dest.TargetDirection, src => SimpleNavigationDto.FromEnum(src.TargetDirection));
    }
}
