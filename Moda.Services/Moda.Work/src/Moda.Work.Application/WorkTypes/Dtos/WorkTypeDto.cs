﻿namespace Moda.Work.Application.WorkTypes.Dtos;
public sealed record WorkTypeDto : IMapFrom<WorkType>
{
    public int Id { get; set; }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>Indicates whether the work type is active or not.</summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; set; }
}
