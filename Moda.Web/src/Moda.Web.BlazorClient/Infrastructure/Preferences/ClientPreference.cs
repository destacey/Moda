using Moda.Web.BlazorClient.Infrastructure.Theme;

namespace Moda.Web.BlazorClient.Infrastructure.Preferences;

public class ClientPreference : IPreference
{
    public bool IsDarkMode { get; set; }
    public bool IsDrawerOpen { get; set; }
    public string PrimaryColor { get; set; } = CustomColors.Light.Primary;
    public string SecondaryColor { get; set; } = CustomColors.Light.Secondary;
    public double BorderRadius { get; set; } = 5;
    public ModaTablePreference TablePreference { get; set; } = new ModaTablePreference();
}
