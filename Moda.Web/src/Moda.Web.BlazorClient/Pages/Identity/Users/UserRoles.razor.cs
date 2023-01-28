﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Moda.Common.Domain.Authorization;
using Moda.Web.BlazorClient.Infrastructure.ApiClient;
using Moda.Web.BlazorClient.Infrastructure.Auth;
using Moda.Web.BlazorClient.Shared;

namespace Moda.Web.BlazorClient.Pages.Identity.Users;

public partial class UserRoles
{
    [Parameter]
    public string? Id { get; set; }
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    protected IUsersClient UsersClient { get; set; } = default!;

    private List<UserRoleDto> _userRolesList = default!;

    private string _title = string.Empty;
    private string _description = string.Empty;

    private string _searchString = string.Empty;

    private bool _canEditUsers;
    private bool _canSearchRoles;
    private bool _loaded;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canEditUsers = await AuthService.HasPermissionAsync(state.User, ApplicationAction.Update, ApplicationResource.Users);
        _canSearchRoles = await AuthService.HasPermissionAsync(state.User, ApplicationAction.View, ApplicationResource.UserRoles);

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => UsersClient.GetByIdAsync(Id), Snackbar)
            is UserDetailsDto user)
        {
            _title = $"{user.FirstName} {user.LastName}";
            _description = string.Format("Manage {0} {1}'s Roles", user.FirstName, user.LastName);

            if (await ApiHelper.ExecuteCallGuardedAsync(
                    () => UsersClient.GetRolesAsync(user.Id.ToString()), Snackbar)
                is ICollection<UserRoleDto> response)
            {
                _userRolesList = response.ToList();
            }
        }

        _loaded = true;
    }

    private async Task SaveAsync()
    {
        var request = new AssignUserRolesRequest()
        {
            UserRoles = _userRolesList
        };

        if (await ApiHelper.ExecuteCallGuardedAsync(
                () => UsersClient.AssignRolesAsync(Id, request),
                Snackbar,
                successMessage: "Updated User Roles.")
            is not null)
        {
            Navigation.NavigateTo("/users");
        }
    }

    private bool Search(UserRoleDto userRole) =>
        string.IsNullOrWhiteSpace(_searchString)
            || userRole.RoleName?.Contains(_searchString, StringComparison.OrdinalIgnoreCase) is true;
}