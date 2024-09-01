using Moda.Common.Application.Employees.Dtos;
using Moda.Common.Application.Services;
using Moda.Common.Domain.Employees;

namespace Moda.Planning.Application.Roadmaps.Dtos;
public sealed record RoadmapDetailsDto : IMapFrom<Roadmap>
{
    /// <summary>Gets or sets the identifier.</summary>
    /// <value>The identifier.</value>
    public Guid Id { get; set; }

    /// <summary>Gets the key.</summary>
    /// <value>The key.</value>
    public int Key { get; set; }

    /// <summary>
    /// The name of the Roadmap.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The description of the Roadmap.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The Roadmap start date.
    /// </summary>
    public required LocalDate Start { get; set; }

    /// <summary>
    /// The Roadmap end date.
    /// </summary>
    public required LocalDate End { get; set; }

    /// <summary>
    /// Indicates if the Roadmap is public.  If true, the Roadmap is visible to all users. If false, the Roadmap is only visible to the managers.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// The managers of the Roadmap.
    /// </summary>
    public required List<EmployeeNavigationDto> Managers { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Roadmap, RoadmapDetailsDto>()
            .Map(dest => dest.Start, src => src.DateRange.Start)
            .Map(dest => dest.End, src => src.DateRange.End)
            .Map(dest => dest.Managers, src => src.Managers.Select(m => EmployeeNavigationDto.From(m.Manager!)));
    }
}
