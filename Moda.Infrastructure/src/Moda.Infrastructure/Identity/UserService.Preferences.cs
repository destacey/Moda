using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Moda.Infrastructure.Identity;

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

        return new UserPreferencesDto
        {
            Tours = user.Preferences.Tours,
        };
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
}
