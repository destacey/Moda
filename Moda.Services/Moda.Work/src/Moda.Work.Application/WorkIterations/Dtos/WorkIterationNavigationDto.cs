using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.WorkIterations.Dtos;
public sealed record WorkIterationNavigationDto : NavigationDto, IMapFrom<WorkIteration>
{
    public static WorkIterationNavigationDto From(WorkIteration workIteration)
        => new()
        {
            Id = workIteration.Id,
            Key = workIteration.Key,
            Name = workIteration.Name
        };

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<WorkIteration, WorkIterationNavigationDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Key, src => src.Key)
            .Map(dest => dest.Name, src => src.Name);
    }
}
