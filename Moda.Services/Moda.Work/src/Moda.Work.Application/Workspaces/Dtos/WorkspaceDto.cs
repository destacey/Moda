using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.Workspaces.Dtos;
public sealed record WorkspaceDto : IMapFrom<Workspace>
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required SimpleNavigationDto Ownership { get; set; }
    public required SimpleNavigationDto WorkProcess { get; set; }
    public Guid? ExternalId { get; set; }
    public bool IsActive { get; set; }

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Workspace, WorkspaceDto>()
            .Map(dest => dest.Key, src => src.Key.ToString())
            .Map(dest => dest.Ownership, src => SimpleNavigationDto.FromEnum(src.Ownership))
            .Map(dest => dest.WorkProcess, src => new SimpleNavigationDto() { Id = src.WorkProcess!.Key, Name = src.WorkProcess.Name });
    }
}
