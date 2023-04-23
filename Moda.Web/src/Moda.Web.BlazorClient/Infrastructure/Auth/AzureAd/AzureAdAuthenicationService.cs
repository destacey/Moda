using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Options;

namespace Moda.Web.BlazorClient.Infrastructure.Auth.AzureAd;

internal class AzureAdAuthenticationService : IAuthenticationService
{
    private readonly NavigationManager _navigation;
    private readonly IOptionsSnapshot<RemoteAuthenticationOptions<ApiAuthorizationProviderOptions>> _optionsSnapshot;

    public AzureAdAuthenticationService(NavigationManager navigation, IOptionsSnapshot<RemoteAuthenticationOptions<ApiAuthorizationProviderOptions>> optionsSnapshot)
    {
        _navigation = navigation;
        _optionsSnapshot = optionsSnapshot;
    }

    public AuthProvider ProviderType => AuthProvider.AzureAd;

    public void NavigateToExternalLogin(string? returnUrl)
    {
        returnUrl = string.IsNullOrWhiteSpace(returnUrl) ? null : $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
        // TODO: the returnUrl is not working
        _navigation.NavigateTo($"{_optionsSnapshot.Get(Options.DefaultName).AuthenticationPaths.LogInPath}{returnUrl}");
    }

    //// not needed at this time because we are only using AzureAd
    public Task<bool> LogInAsync() =>
        throw new NotImplementedException();

    public Task LogOutAsync()
    {
        _navigation.NavigateToLogout("authentication/logout");
        return Task.CompletedTask;
    }

    public Task ReLoginAsync(string? returnUrl)
    {
        NavigateToExternalLogin(returnUrl);
        return Task.CompletedTask;
    }
}
