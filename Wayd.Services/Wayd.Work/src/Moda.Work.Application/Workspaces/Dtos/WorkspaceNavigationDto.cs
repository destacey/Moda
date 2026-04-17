using Wayd.Common.Application.Dtos;

namespace Wayd.Work.Application.Workspaces.Dtos;

public sealed record WorkspaceNavigationDto : NavigationDto<Guid, string>, IMapFrom<Workspace>
{

}
