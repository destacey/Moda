using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Moda.Common.Domain.Authorization;
using Moda.Web.BlazorClient.Infrastructure.ApiClient;
using Moda.Web.BlazorClient.Infrastructure.Auth;
using MudBlazor;

namespace Moda.Web.BlazorClient.Pages.Identity.Users;

public partial class Users
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthState { get; set; } = default!;
    [Inject]
    protected IAuthorizationService AuthService { get; set; } = default!;

    [Inject]
    protected IUsersClient UsersClient { get; set; } = default!;

    //protected EntityClientTableContext<UserDetailsDto, Guid, CreateUserRequest> Context { get; set; } = default!;

    private bool _canExportUsers;
    private bool _canViewRoles;

    // Fields for editform
    protected string Password { get; set; } = string.Empty;
    protected string ConfirmPassword { get; set; } = string.Empty;

    //private bool _passwordVisibility;
    //private InputType _passwordInput = InputType.Password;
    //private string _passwordInputIcon = Icons.Material.Filled.VisibilityOff;

    protected override async Task OnInitializedAsync()
    {
        var user = (await AuthState).User;
        _canExportUsers = await AuthService.HasPermissionAsync(user, ApplicationAction.Export, ApplicationResource.Users);
        _canViewRoles = await AuthService.HasPermissionAsync(user, ApplicationAction.View, ApplicationResource.UserRoles);

        //Context = new(
        //    entityName: "User",
        //    entityNamePlural: "Users",
        //    entityResource: ApplicationResource.Users,
        //    searchAction: ApplicationAction.View,
        //    updateAction: string.Empty,
        //    deleteAction: string.Empty,
        //    fields: new()
        //    {
        //        new(user => user.FirstName, "First Name"),
        //        new(user => user.LastName, "Last Name"),
        //        new(user => user.UserName, "UserName"),
        //        new(user => user.Email, "Email"),
        //        new(user => user.PhoneNumber, "PhoneNumber"),
        //       // new(user => user.EmailConfirmed, "Email Confirmation", Type: typeof(bool)),
        //        new(user => user.IsActive, "Active", Type: typeof(bool))
        //    },
        //    idFunc: user => new Guid(user.Id),
        //    loadDataFunc: async () => (await UsersClient.GetListAsync()).ToList(),
        //    searchFunc: (searchString, user) =>
        //        string.IsNullOrWhiteSpace(searchString)
        //            || user.FirstName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true
        //            || user.LastName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true
        //            || user.Email?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true
        //            || user.PhoneNumber?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true
        //            || user.UserName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) == true,
        //    //createFunc: user => UsersClient.CreateAsync(user),
        //    hasExtraActionsFunc: () => true,
        //    exportAction: string.Empty);
    }

    private void ViewProfile(in Guid userId) =>
        Navigation.NavigateTo($"/users/{userId}/profile");

    private void ManageRoles(in Guid userId) =>
        Navigation.NavigateTo($"/users/{userId}/roles");

    private void TogglePasswordVisibility()
    {
        //if (_passwordVisibility)
        //{
        //    _passwordVisibility = false;
        //    _passwordInputIcon = Icons.Material.Filled.VisibilityOff;
        //    _passwordInput = InputType.Password;
        //}
        //else
        //{
        //    _passwordVisibility = true;
        //    _passwordInputIcon = Icons.Material.Filled.Visibility;
        //    _passwordInput = InputType.Text;
        //}

        //Context.AddEditModal.ForceRender();
    }
}

// TODO delete this was created to delay fixing a missing action
public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}