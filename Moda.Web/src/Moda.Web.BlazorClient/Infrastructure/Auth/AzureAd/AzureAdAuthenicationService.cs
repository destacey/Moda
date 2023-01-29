using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;

namespace Moda.Web.BlazorClient.Infrastructure.Auth.AzureAd;

internal class AzureAdAuthenticationService : IAuthenticationService
{
    private readonly NavigationManager _navigation;

    public AzureAdAuthenticationService(NavigationManager navigation) =>
        (_navigation) = (navigation);

    public AuthProvider ProviderType => AuthProvider.AzureAd;

    public void NavigateToExternalLogin(string returnUrl) =>
        _navigation.NavigateTo($"authentication/login?returnUrl={Uri.EscapeDataString(returnUrl)}");


    //// not needed at this time because we are only using AzureAd
    public Task<bool> LogInAsync() =>
        throw new NotImplementedException();

    public Task LogOutAsync()
    {
        _navigation.NavigateToLogout("authentication/logout");
        return Task.CompletedTask;
    }

    public Task ReLoginAsync(string returnUrl)
    {
        NavigateToExternalLogin(returnUrl);
        return Task.CompletedTask;
    }
}
