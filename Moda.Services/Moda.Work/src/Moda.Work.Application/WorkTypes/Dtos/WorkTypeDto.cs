using Moda.Common.Application.Dtos;
using Moda.Common.Application.Requests.WorkManagement.Interfaces;

namespace Moda.Work.Application.WorkTypes.Dtos;

public sealed record WorkTypeDto : IMapFrom<WorkType>, IWorkTypeDto
{
    public int Id { get; set; }

    /// <summary>
    /// The name of the work type.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of the work type.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The work type level name.
    /// </summary>
    public required SimpleNavigationDto Level { get; set; }

    /// <summary>
    /// Indicates whether the work type is active.
    /// </summary>
    public bool IsActive { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkType, WorkTypeDto>()
            .Map(dest => dest.Level, src => SimpleNavigationDto.Create(src.LevelId, src.Level!.Name));
    }
}
