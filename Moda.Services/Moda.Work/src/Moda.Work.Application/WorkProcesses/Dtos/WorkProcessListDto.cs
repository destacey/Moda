using Mapster;
using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.WorkProcesses.Dtos;
public sealed record WorkProcessListDto : IMapFrom<WorkProcess>
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>The name of the work status.  The name cannot be changed.</summary>
    /// <value>The name.</value>
    public required string Name { get; set; }

    /// <summary>The description of the work status.</summary>
    /// <value>The description.</value>
    public string? Description { get; set; }

    public required SimpleNavigationDto Ownership { get; set; }

    /// <summary>Indicates whether the work status is active or not.</summary>
    /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
    public bool IsActive { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkProcess, WorkProcessListDto>()
            .Map(dest => dest.Ownership, src => SimpleNavigationDto.FromEnum(src.Ownership));
    }
}
