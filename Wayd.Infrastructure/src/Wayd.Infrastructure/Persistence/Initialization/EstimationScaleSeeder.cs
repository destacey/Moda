using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Wayd.Planning.Domain.Models.PlanningPoker;

namespace Wayd.Infrastructure.Persistence.Initialization;

public class EstimationScaleSeeder : ICustomSeeder
{
    public async Task Initialize(WaydDbContext dbContext, IDateTimeProvider dateTimeProvider, CancellationToken cancellationToken)
    {
        if (await dbContext.EstimationScales.AnyAsync(cancellationToken))
            return;

        var seeds = new[]
        {
            CreateScale("Fibonacci", "Fibonacci sequence values commonly used in agile estimation.",
                ["0", "1", "2", "3", "5", "8", "13", "21", "?", "∞", "☕"]),
            CreateScale("Modified Fibonacci", "Modified Fibonacci sequence.",
                ["0", "1", "2", "3", "5", "8", "13", "20", "40", "100", "?", "∞", "☕"]),
            CreateScale("T-Shirt Sizes", "Simple relative sizing using t-shirt sizes.",
                ["XXS", "XS", "S", "M", "L", "XL", "XXL", "?", "∞", "☕"]),
            CreateScale("Simple", "Simple 1-5 scale for quick estimation.",
                ["1", "2", "3", "4", "5", "?", "∞", "☕"]),
        };

        foreach (var seed in seeds)
        {
            if (seed.IsSuccess)
            {
                dbContext.EstimationScales.Add(seed.Value);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static Result<EstimationScale> CreateScale(string name, string description, string[] values)
    {
        return EstimationScale.Create(name, description, values);
    }
}
