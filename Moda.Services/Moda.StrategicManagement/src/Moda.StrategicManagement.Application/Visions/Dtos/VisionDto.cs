using Moda.Common.Application.Dtos;
using Moda.StrategicManagement.Domain.Models;

namespace Moda.StrategicManagement.Application.Visions.Dtos;
public sealed record VisionDto : IMapFrom<Vision>
{
    /// <summary>
    /// The unique identifier of the vision.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the vision.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// A concise statement describing the vision of the organization.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The current lifecycle state of the vision.
    /// </summary>
    public required SimpleNavigationDto State { get; set; }

    /// <summary>
    /// The date when the vision became active or started guiding the organization.
    /// </summary>
    public LocalDate? Start { get; set; }

    /// <summary>
    /// The date when the vision was archived or replaced.
    /// </summary>
    public LocalDate? End { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Vision, VisionDto>()
            .Map(dest => dest.State, src => SimpleNavigationDto.FromEnum(src.State));
    }
}
