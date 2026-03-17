using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Moda.Common.Domain.FeatureManagement;

namespace Moda.Infrastructure.Persistence.Initialization;

public class FeatureFlagSeeder : ICustomSeeder
{
    public async Task Initialize(ModaDbContext dbContext, IDateTimeProvider dateTimeProvider, CancellationToken cancellationToken)
    {
        var definitions = GetAllDefinitions();
        if (definitions.Length == 0)
            return;

        var existingFlags = await dbContext.FeatureFlags
            .ToListAsync(cancellationToken);

        var existingByName = existingFlags.ToDictionary(f => f.Name, StringComparer.OrdinalIgnoreCase);
        var hasChanges = false;

        foreach (var definition in definitions)
        {
            if (existingByName.TryGetValue(definition.Name, out var existing))
            {
                // Sync metadata from the code definition without touching IsEnabled.
                // IsEnabled is admin-controlled and must never be reset by the seeder.
                if (existing.DisplayName != definition.DisplayName || existing.Description != definition.Description)
                {
                    existing.Update(definition.DisplayName, definition.Description);
                    hasChanges = true;
                }
            }
            else
            {
                var result = FeatureFlag.Create(definition.Name, definition.DisplayName, definition.Description, false, isSystem: true);
                if (result.IsSuccess)
                {
                    dbContext.FeatureFlags.Add(result.Value);
                    hasChanges = true;
                }
            }
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static FeatureFlagDefinition[] GetAllDefinitions()
    {
        return typeof(FeatureFlags)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(FeatureFlagDefinition))
            .Select(f => (FeatureFlagDefinition)f.GetValue(null)!)
            .ToArray();
    }
}
