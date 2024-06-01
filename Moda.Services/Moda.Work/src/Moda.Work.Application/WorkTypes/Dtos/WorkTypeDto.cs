using Moda.Common.Application.Requests.WorkManagement.Interfaces;

namespace Moda.Work.Application.WorkTypes.Dtos;

public sealed record WorkTypeDto : IMapFrom<WorkType>, IWorkTypeDto
{
    public int Id { get; set; }

    /// <summary>The name of the work type.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work type.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    /// <summary>
    /// The work type level name.
    /// </summary>
    public required string Level { get; set; }

    /// <summary>Indicates whether the work type is active or not.</summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkType, WorkTypeDto>()
            .Map(dest => dest.Level, src => src.Level!.Name);
    }
}
