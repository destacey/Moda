﻿using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Domain.Models;
public sealed class UpsertPlanningIntervalIteration
{
    public Guid? Id { get; set; }
    public required string Name { get; set; }
    public IterationType Type { get; set; }
    public required LocalDateRange DateRange { get; set; }
    public bool IsNew => Id.IsNullEmptyOrDefault();

    public static UpsertPlanningIntervalIteration Create(Guid? id, string name, IterationType type, LocalDateRange dateRange)
    {
        return new UpsertPlanningIntervalIteration
        {
            Id = id,
            Name = name,
            Type = type,
            DateRange = dateRange
        };
    }
}
