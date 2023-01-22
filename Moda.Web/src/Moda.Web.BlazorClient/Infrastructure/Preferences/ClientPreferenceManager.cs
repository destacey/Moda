﻿using System.Text.RegularExpressions;
using Blazored.LocalStorage;
using Moda.Web.BlazorClient.Infrastructure.Theme;
using MudBlazor;

namespace Moda.Web.BlazorClient.Infrastructure.Preferences;

public partial class ClientPreferenceManager : IClientPreferenceManager
{
    private readonly ILocalStorageService _localStorageService;

    public ClientPreferenceManager(
        ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }

    public async Task<bool> ToggleDarkModeAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            preference.IsDarkMode = !preference.IsDarkMode;
            await SetPreference(preference);
            return !preference.IsDarkMode;
        }

        return false;
    }

    public async Task<bool> ToggleDrawerAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            preference.IsDrawerOpen = !preference.IsDrawerOpen;
            await SetPreference(preference);
            return preference.IsDrawerOpen;
        }

        return false;
    }

    public async Task<bool> ToggleLayoutDirectionAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            preference.IsRTL = !preference.IsRTL;
            await SetPreference(preference);
            return preference.IsRTL;
        }

        return false;
    }

    public async Task<MudTheme> GetCurrentThemeAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            if (preference.IsDarkMode) return new DarkTheme();
        }

        return new LightTheme();
    }

    public async Task<string> GetPrimaryColorAsync()
    {
        if (await GetPreference() is ClientPreference preference)
        {
            string colorCode = preference.PrimaryColor;
            if (ColorCodeMatchRegex().Match(colorCode).Success)
            {
                return colorCode;
            }
            else
            {
                preference.PrimaryColor = CustomColors.Light.Primary;
                await SetPreference(preference);
                return preference.PrimaryColor;
            }
        }

        return CustomColors.Light.Primary;
    }

    public async Task<bool> IsRTL()
    {
        return await GetPreference() is ClientPreference preference ? preference.IsRTL : false;
    }

    public async Task<bool> IsDrawerOpen()
    {
        return await GetPreference() is ClientPreference preference ? preference.IsDrawerOpen : false;
    }

    public static string Preference = "clientPreference";

    public async Task<IPreference> GetPreference()
    {
        return await _localStorageService.GetItemAsync<ClientPreference>(Preference) ?? new ClientPreference();
    }

    public async Task SetPreference(IPreference preference)
    {
        await _localStorageService.SetItemAsync(Preference, preference as ClientPreference);
    }

    [GeneratedRegex("^#(?:[0-9a-fA-F]{3,4}){1,2}$")]
    private static partial Regex ColorCodeMatchRegex();
}