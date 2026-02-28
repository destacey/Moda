using Microsoft.EntityFrameworkCore;
using Moda.Planning.Domain.Models.PlanningPoker;
using NodaTime;

namespace Moda.Infrastructure.Persistence.Initialization;

public class EstimationScaleSeeder : ICustomSeeder
{
    public async Task Initialize(ModaDbContext dbContext, IDateTimeProvider dateTimeProvider, CancellationToken cancellationToken)
    {
        if (await dbContext.EstimationScales.AnyAsync(s => s.IsPreset, cancellationToken))
            return;

        Instant timestamp = dateTimeProvider.Now;

        var presets = new[]
        {
            CreatePreset("Fibonacci", "Fibonacci sequence values commonly used in agile estimation.",
                ["0", "1", "2", "3", "5", "8", "13", "21", "?"], timestamp),
            CreatePreset("T-Shirt Sizes", "Simple relative sizing using t-shirt sizes.",
                ["XS", "S", "M", "L", "XL", "?"], timestamp),
            CreatePreset("Powers of 2", "Powers of two for exponential estimation.",
                ["0", "1", "2", "4", "8", "16", "32", "?"], timestamp),
            CreatePreset("Simple", "Simple 1-5 scale for quick estimation.",
                ["1", "2", "3", "4", "5", "?"], timestamp),
        };

        foreach (var preset in presets)
        {
            if (preset.IsSuccess)
            {
                dbContext.EstimationScales.Add(preset.Value);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static CSharpFunctionalExtensions.Result<EstimationScale> CreatePreset(string name, string description, string[] values, Instant timestamp)
    {
        var scaleValues = values.Select((v, i) => new { Value = v, Order = i }).ToList();
        return EstimationScale.CreatePreset(
            name,
            description,
            scaleValues.Select(sv => (sv.Value, sv.Order)).ToList());
    }
}
