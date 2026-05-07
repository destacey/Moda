using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Wayd.Infrastructure.Identity;

internal partial class UserService
{
    public async Task<UserPreferencesDto> GetPreferences(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User with id {UserId} not found.", userId);
            throw new NotFoundException("User Not Found.");
        }

        return MapToDto(user.Preferences);
    }

    public async Task<Result> UpdatePreferences(string userId, UserPreferencesDto preferences, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User with id {UserId} not found.", userId);
            return Result.Failure("User not found.");
        }

        user.Preferences = new UserPreferences
        {
            Tours = new Dictionary<string, bool>(preferences.Tours),
            ThemeConfig = preferences.ThemeConfig is null ? null : new UserThemeConfig
            {
                ColorPrimary = preferences.ThemeConfig.ColorPrimary,
                UseCompactAlgorithm = preferences.ThemeConfig.UseCompactAlgorithm,
            },
        };

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Error updating preferences for user {UserId}: {Errors}", userId, errors);
            return Result.Failure(errors);
        }

        return Result.Success();
    }

    public async Task<UserThemeConfigDto?> GetThemeConfig(string userId, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User with id {UserId} not found.", userId);
            throw new NotFoundException("User Not Found.");
        }

        return MapThemeConfigToDto(user.Preferences.ThemeConfig);
    }

    public async Task<Result> UpdateThemeConfig(string userId, UserThemeConfigDto? themeConfig, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            _logger.LogInformation("User with id {UserId} not found.", userId);
            return Result.Failure("User not found.");
        }

        user.Preferences = new UserPreferences
        {
            Tours = user.Preferences.Tours,
            ThemeConfig = themeConfig is null ? null : new UserThemeConfig
            {
                ColorPrimary = themeConfig.ColorPrimary,
                UseCompactAlgorithm = themeConfig.UseCompactAlgorithm,
            },
        };

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Error updating theme config for user {UserId}: {Errors}", userId, errors);
            return Result.Failure(errors);
        }

        return Result.Success();
    }

    private static UserPreferencesDto MapToDto(UserPreferences preferences) =>
        new()
        {
            Tours = preferences.Tours,
            ThemeConfig = MapThemeConfigToDto(preferences.ThemeConfig),
        };

    private static UserThemeConfigDto? MapThemeConfigToDto(UserThemeConfig? themeConfig) =>
        themeConfig is null ? null : new UserThemeConfigDto
        {
            ColorPrimary = themeConfig.ColorPrimary,
            UseCompactAlgorithm = themeConfig.UseCompactAlgorithm,
        };
}
