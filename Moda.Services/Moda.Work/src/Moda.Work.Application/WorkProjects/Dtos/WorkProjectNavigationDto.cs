using System.ComponentModel.DataAnnotations;

namespace Moda.Work.Application.WorkProjects.Dtos;
public sealed record WorkProjectNavigationDto : IMapFrom<WorkProject>
{
    [Required]
    public required Guid Id { get; set; }

    [Required]
    public required string Key { get; set; }

    [Required]
    public required string Name { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkProject, WorkProjectNavigationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Key, src => src.Key.Value)
            .Map(dest => dest.Name, src => src.Name);
    }
}
