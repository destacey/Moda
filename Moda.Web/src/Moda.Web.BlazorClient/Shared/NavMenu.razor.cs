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
    private bool _canViewHangfire;
    private bool _canViewUsers;
    private bool _canViewRoles;
    private bool CanViewAdministrationGroup => _canViewUsers || _canViewRoles;
    
    protected override async Task OnInitializedAsync()
    {
        _hangfireUrl = Config[ConfigNames.ApiBaseUrl] + "jobs";
        var user = (await AuthState).User;
        _canViewHangfire = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.Hangfire);
        _canViewRoles = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.Roles);
        _canViewUsers = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.Users);
    }
}