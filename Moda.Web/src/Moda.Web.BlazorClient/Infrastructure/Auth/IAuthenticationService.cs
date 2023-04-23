namespace Moda.Web.BlazorClient.Infrastructure.Auth;

public interface IAuthenticationService
{
    AuthProvider ProviderType { get; }

    void NavigateToExternalLogin(string? returnUrl);


    //// not needed at this time because we are only using AzureAd
    Task<bool> LogInAsync();

    Task LogOutAsync();

    Task ReLoginAsync(string? returnUrl);
}
