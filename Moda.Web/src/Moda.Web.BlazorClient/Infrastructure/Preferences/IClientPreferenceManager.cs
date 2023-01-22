using MudBlazor;

namespace Moda.Web.BlazorClient.Infrastructure.Preferences;

public interface IClientPreferenceManager : IPreferenceManager
{
    Task<MudTheme> GetCurrentThemeAsync();

    Task<bool> ToggleDarkModeAsync();

    Task<bool> ToggleDrawerAsync();

    Task<bool> ToggleLayoutDirectionAsync();
}
