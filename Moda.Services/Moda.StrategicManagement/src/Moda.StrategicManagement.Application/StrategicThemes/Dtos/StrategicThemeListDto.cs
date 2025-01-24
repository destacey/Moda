using Moda.Common.Application.Dtos;
using Moda.StrategicManagement.Domain.Models;

namespace Moda.StrategicManagement.Application.StrategicThemes.Dtos;
public sealed record StrategicThemeListDto : IMapFrom<StrategicTheme>
{
    /// <summary>
    /// The unique identifier of the StrategicTheme.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The unique key of the StrategicTheme.  This is an alternate key to the Id.
    /// </summary>
    public int Key { get; set; }

    /// <summary>
    /// The name of the strategic theme, highlighting its focus or priority.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The current lifecycle state of the strategic theme.
    /// </summary>
    public required SimpleNavigationDto State { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<StrategicTheme, StrategicThemeListDto>()
            .Map(dest => dest.State, src => SimpleNavigationDto.FromEnum(src.State));
    }
}
