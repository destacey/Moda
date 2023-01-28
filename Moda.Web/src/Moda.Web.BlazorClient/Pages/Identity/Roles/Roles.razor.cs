using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Moda.Common.Domain.Authorization;
using Moda.Web.BlazorClient.Infrastructure.ApiClient;
using Moda.Web.BlazorClient.Infrastructure.Auth;

namespace Moda.Web.BlazorClient.Pages.Identity.Roles;

public partial class Roles
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;
    [Inject]
    private IRolesClient RolesClient { get; set; } = default!;

    //protected EntityClientTableContext<RoleDto, string?, CreateOrUpdateRoleRequest> Context { get; set; } = default!;

    private bool _canViewRoleClaims;

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthState;
        _canViewRoleClaims = await AuthService.HasPermissionAsync(state.User, ApplicationAction.View, ApplicationResource.RoleClaims);

        //Context = new(
        //    entityName: "Role",
        //    entityNamePlural: "Roles",
        //    entityResource: ApplicationResource.Roles,
        //    searchAction: ApplicationAction.View,
        //    fields: new()
        //    {
        //        new(role => role.Id, "Id"),
        //        new(role => role.Name, "Name"),
        //        new(role => role.Description, "Description")
        //    },
        //    idFunc: role => role.Id,
        //    loadDataFunc: async () => (await RolesClient.GetListAsync()).ToList(),
        //    searchFunc: (searchString, role) =>
        //        string.IsNullOrWhiteSpace(searchString)
        //            || role.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true
        //            || role.Description?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true,
        //    createFunc: async role => await RolesClient.CreateAsync(role),
        //    updateFunc: async (_, role) => await RolesClient.CreateAsync(role),
        //    deleteFunc: async id => await RolesClient.DeleteAsync(id),
        //    hasExtraActionsFunc: () => _canViewRoleClaims,
        //    canUpdateEntityFunc: e => !ApplicationRoles.IsDefault(e.Name),
        //    canDeleteEntityFunc: e => !ApplicationRoles.IsDefault(e.Name),
        //    exportAction: string.Empty);
    }

    private void ManagePermissions(string? roleId)
    {
        ArgumentNullException.ThrowIfNull(roleId, nameof(roleId));
        Navigation.NavigateTo($"/roles/{roleId}/permissions");
    }
}