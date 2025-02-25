using Moda.Common.Extensions;
using Moda.StrategicManagement.Domain.Models;

namespace Moda.StrategicManagement.Application.StrategicThemes.Dtos;
public sealed record StrategicThemeOptionDto : IMapFrom<StrategicTheme>
{
    /// <summary>
    /// The unique identifier of the StrategicTheme.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the strategic theme, highlighting its focus or priority.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The current lifecycle state of the strategic theme.
    /// </summary>
    public required string State { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<StrategicTheme, StrategicThemeOptionDto>()
            .Map(dest => dest.State, src => src.State.GetDisplayName());
    }
}
