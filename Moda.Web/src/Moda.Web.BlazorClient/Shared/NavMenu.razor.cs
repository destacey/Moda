using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Moda.Common.Domain.Authorization;
using Moda.Web.BlazorClient.Infrastructure.Auth;

namespace Moda.Web.BlazorClient.Shared;

public partial class NavMenu
{
    [CascadingParameter]
    public Task<AuthenticationState> AuthState { get; set; } = default!;


    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    private bool _canViewUsers;
    private bool _canViewRoles;
    private bool _canViewBackgroundJobs;
    private bool _canViewAdministrationGroup => _canViewUsers || _canViewRoles || _canViewBackgroundJobs;
    
    protected override async Task OnInitializedAsync()
    {
        var user = (await AuthState).User;
        _canViewUsers = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.Users);
        _canViewRoles = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.Roles);
        _canViewBackgroundJobs = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.BackgroundJobs);
    }
}