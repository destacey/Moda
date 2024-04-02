using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.Workspaces.Dtos;
public sealed record WorkspaceListDto : IMapFrom<Workspace>
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public required SimpleNavigationDto Ownership { get; set; }
    public bool IsActive { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Workspace, WorkspaceListDto>()
            .Map(dest => dest.Key, src => src.Key.ToString())
            .Map(dest => dest.Ownership, src => SimpleNavigationDto.FromEnum(src.Ownership));
    }
}
