using MudBlazor;

namespace Moda.Web.BlazorClient.Infrastructure.Theme;

public class DarkTheme : MudTheme
{
    public DarkTheme()
    {
        Palette = new PaletteDark()
        {
            // PaletteDark Overrides
            Black = "#27272f",
            Primary = CustomColors.Dark.Primary,
            Info = "#3299ff",
            Success = CustomColors.Dark.Primary,
            Warning = "#ffa800",
            Error = "#f64e62",
            Dark = CustomColors.Dark.DrawerBackground,
            TextPrimary = "rgba(255,255,255, 0.70)",
            TextSecondary = "rgba(255,255,255, 0.50)",
            TextDisabled = CustomColors.Dark.Disabled,
            ActionDefault = "#adadb1",
            ActionDisabled = "rgba(255,255,255, 0.26)",
            ActionDisabledBackground = "rgba(255,255,255, 0.12)",
            Background = CustomColors.Dark.Background,
            BackgroundGrey = "#27272f",
            Surface = CustomColors.Dark.Surface,
            DrawerBackground = CustomColors.Dark.DrawerBackground,
            DrawerText = "rgba(255,255,255, 0.50)",
            DrawerIcon = "rgba(255,255,255, 0.50)",
            AppbarBackground = CustomColors.Dark.AppbarBackground,
            AppbarText = "rgba(255,255,255, 0.70)",
            LinesDefault = "rgba(255,255,255, 0.12)",
            LinesInputs = "rgba(255,255,255, 0.3)",
            TableLines = "rgba(255,255,255, 0.12)",
            TableStriped = "rgba(255,255,255, 0.06)",
            Divider = "rgba(255,255,255, 0.12)",
            DividerLight = "rgba(255,255,255, 0.06)",

            // Palette Overrides
            OverlayDark = "hsl(0deg 0% 0% / 75%)",
            Secondary = CustomColors.Dark.Secondary,
            HoverOpacity = 0.40,
            TableHover = "rgba(255,255,255, 0.25)"
        };

        LayoutProperties = new LayoutProperties()
        {
            DefaultBorderRadius = "5px",
        };

        Typography = CustomTypography.ModaTypography;
        Shadows = new Shadow();
        ZIndex = new ZIndex() { Drawer = 1300 };
    }
}