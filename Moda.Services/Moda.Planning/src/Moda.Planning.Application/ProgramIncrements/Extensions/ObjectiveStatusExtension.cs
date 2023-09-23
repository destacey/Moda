﻿using Moda.Planning.Domain.Enums;

namespace Moda.Planning.Application.ProgramIncrements.Extensions;
public static class ObjectiveStatusExtension
{
    public static Goals.Domain.Enums.ObjectiveStatus ToGoalObjectiveStatus(this ObjectiveStatus status)
    {
        return status switch
        {
            ObjectiveStatus.NotStarted => Goals.Domain.Enums.ObjectiveStatus.NotStarted,
            ObjectiveStatus.InProgress => Goals.Domain.Enums.ObjectiveStatus.InProgress,
            ObjectiveStatus.Closed => Goals.Domain.Enums.ObjectiveStatus.Closed,
            ObjectiveStatus.Canceled => Goals.Domain.Enums.ObjectiveStatus.Canceled,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
}