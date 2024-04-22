using Moda.Common.Application.Dtos;

namespace Moda.Work.Application.Workspaces.Dtos;
public sealed record WorkspaceNavigationDto : NavigationDto<Guid,string>, IMapFrom<Workspace>
{

}
