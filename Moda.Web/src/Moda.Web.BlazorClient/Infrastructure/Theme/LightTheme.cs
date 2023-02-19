using MudBlazor;

namespace Moda.Web.BlazorClient.Infrastructure.Theme;

public class LightTheme : MudTheme
{
    public LightTheme()
    {
        Palette = new Palette()
        {
            Primary = CustomColors.Light.Primary,
            Secondary = CustomColors.Light.Secondary,
            Success = CustomColors.Light.Primary,
            Background = CustomColors.Light.Background,
            DrawerBackground = CustomColors.Light.DrawerBackground,
            DrawerText = "rgba(0,0,0, 0.7)",
            AppbarBackground = CustomColors.Light.AppbarBackground,
            AppbarText = CustomColors.Light.AppbarText,
            TableLines = "#e0e0e029",
            OverlayDark = "hsl(0deg 0% 0% / 75%)"
        };
        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "5px"
        };

        Typography = CustomTypography.ModaTypography;
        Shadows = new Shadow();
        ZIndex = new ZIndex() { Drawer = 1300 };
    }
}