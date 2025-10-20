using System.Diagnostics;
using Moda.Integrations.AzureDevOps.Utils;
using Xunit.Abstractions;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Utils;

[Collection("Performance Tests")]
public class ClassificationPathUtilsPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public ClassificationPathUtilsPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void RemoveClassificationTypeFromPath_Performance_MinimizesAllocations()
    {
        // Arrange - Simulate processing 5000+ iterations
        var paths = new List<string>(5000);
        for (int i = 0; i < 5000; i++)
        {
            paths.Add($"\\Project{i % 10}\\Iteration\\Team{i % 50}\\2024\\Sprint{i % 20}");
        }

        // Warm-up: ensure JIT optimizes before measuring
        foreach (var path in paths)
            _ = ClassificationNodeUtils.RemoveClassificationTypeFromPath(path);

        // Act - Measure allocations would require a benchmark tool, but we can verify it completes quickly
        var startTime = Stopwatch.GetTimestamp();
        
        foreach (var path in paths)
            _ = ClassificationNodeUtils.RemoveClassificationTypeFromPath(path);

        var elapsedMilliseconds = Stopwatch.GetElapsedTime(startTime).TotalMilliseconds;

        _output.WriteLine($"Execution time: {elapsedMilliseconds:F3} ms");

        // Assert - Should complete very quickly (< 2ms for 5000 iterations)
        // this will typically run in under 1ms on a modern machine, but 2ms is a safe upper bound
        // this is machine dependent but gives a rough idea
        elapsedMilliseconds.Should().BeLessThan(2);
    }
}
