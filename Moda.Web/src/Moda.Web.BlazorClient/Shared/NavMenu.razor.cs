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

    private string? _hangfireUrl;
    private bool _canViewUsers;
    private bool _canViewRoles;
    private bool _canViewBackgroundJobs;
    private bool _canViewHangfire;
    private bool CanViewAdministrationGroup => _canViewUsers || _canViewRoles || _canViewBackgroundJobs || _canViewHangfire;
    
    protected override async Task OnInitializedAsync()
    {
        _hangfireUrl = Config[ConfigNames.ApiBaseUrl] + "jobs";
        var user = (await AuthState).User;
        _canViewUsers = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.Users);
        _canViewRoles = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.Roles);
        _canViewBackgroundJobs = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.BackgroundJobs);
        _canViewHangfire = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.Hangfire);
    }
}