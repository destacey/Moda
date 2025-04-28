using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.WorkProjects.Dtos;
public sealed record WorkProjectNavigationDto : NavigationDto, IMapFrom<WorkProject>
{
    public static WorkProjectNavigationDto From(WorkProject workProject)
        => new()
        {
            Id = workProject.Id,
            Key = workProject.Key,
            Name = workProject.Name
        };

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkProject, WorkProjectNavigationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Key, src => src.Key)
            .Map(dest => dest.Name, src => src.Name);
    }
}
