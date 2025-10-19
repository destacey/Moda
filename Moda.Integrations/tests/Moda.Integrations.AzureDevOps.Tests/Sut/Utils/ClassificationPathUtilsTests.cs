using Moda.Integrations.AzureDevOps.Utils;

namespace Moda.Integrations.AzureDevOps.Tests.Sut.Utils;

public class ClassificationPathUtilsTests
{
    [Theory]
    [InlineData("\\Moda\\Iteration", "\\Moda")]
    [InlineData("\\Moda\\Area", "\\Moda")]
    [InlineData("\\ProjectName\\Iteration", "\\ProjectName")]
    public void RemoveClassificationTypeFromPath_WithRootPath_ReturnsProjectOnly(string input, string expected)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\\Moda\\Iteration\\Team Moda", "\\Moda\\Team Moda")]
    [InlineData("\\Moda\\Iteration\\Moda Integrations Team\\Test", "\\Moda\\Moda Integrations Team\\Test")]
    [InlineData("\\Moda\\Area\\Product\\Feature", "\\Moda\\Product\\Feature")]
    public void RemoveClassificationTypeFromPath_WithSingleLevelHierarchy_RemovesClassificationType(string input, string expected)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\\Moda\\Iteration\\Team Moda\\2022\\22.3\\22.3.6", "\\Moda\\Team Moda\\2022\\22.3\\22.3.6")]
    [InlineData("\\Moda\\Iteration\\Team Moda\\2022", "\\Moda\\Team Moda\\2022")]
    [InlineData("\\Moda\\Area\\Product\\Feature\\SubFeature\\Component", "\\Moda\\Product\\Feature\\SubFeature\\Component")]
    public void RemoveClassificationTypeFromPath_WithDeepHierarchy_RemovesClassificationType(string input, string expected)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("\\Moda\\Iteration\\Team Iteration Name", "\\Moda\\Team Iteration Name")]
    [InlineData("\\Moda\\Area\\Area Team\\Iteration Area", "\\Moda\\Area Team\\Iteration Area")]
    public void RemoveClassificationTypeFromPath_WithClassificationTypeInHierarchyNames_OnlyRemovesSecondNode(string input, string expected)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Moda")]
    [InlineData("\\Moda")]
    public void RemoveClassificationTypeFromPath_WithInvalidPath_ReturnsOriginalPath(string input)
    {
        // Act
        var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(input);

        // Assert
        result.Should().Be(input);
    }

    [Fact]
    public void RemoveClassificationTypeFromPath_WithRealWorldIterationPaths_TransformsCorrectly()
    {
        // Arrange - Real paths from Azure DevOps
        var testCases = new Dictionary<string, string>
        {
            { "\\Moda\\Iteration", "\\Moda" },
            { "\\Moda\\Iteration\\Moda Integrations Team", "\\Moda\\Moda Integrations Team" },
            { "\\Moda\\Iteration\\Moda Integrations Team\\Test", "\\Moda\\Moda Integrations Team\\Test" },
            { "\\Moda\\Iteration\\Team Moda", "\\Moda\\Team Moda" },
            { "\\Moda\\Iteration\\Team Moda\\2022", "\\Moda\\Team Moda\\2022" },
            { "\\Moda\\Iteration\\Team Moda\\2022\\22.3", "\\Moda\\Team Moda\\2022\\22.3" },
            { "\\Moda\\Iteration\\Team Moda\\2022\\22.3\\22.3.6", "\\Moda\\Team Moda\\2022\\22.3\\22.3.6" },
            { "\\Moda\\Iteration\\Team Moda\\2022\\22.3\\22.3.7", "\\Moda\\Team Moda\\2022\\22.3\\22.3.7" },
            { "\\Moda\\Iteration\\Team Moda\\2022\\22.4", "\\Moda\\Team Moda\\2022\\22.4" },
            { "\\Moda\\Iteration\\Team Moda\\2022\\22.4\\22.4.1", "\\Moda\\Team Moda\\2022\\22.4\\22.4.1" },
            { "\\Moda\\Iteration\\Team Moda\\2022\\22.4\\22.4.2", "\\Moda\\Team Moda\\2022\\22.4\\22.4.2" },
            { "\\Moda\\Iteration\\Team Moda\\2022\\22.4\\22.4.3", "\\Moda\\Team Moda\\2022\\22.4\\22.4.3" },
            { "\\Moda\\Iteration\\Team Moda\\2023", "\\Moda\\Team Moda\\2023" },
            { "\\Moda\\Iteration\\Team Moda\\2024", "\\Moda\\Team Moda\\2024" }
        };

        // Act & Assert
        foreach (var testCase in testCases)
        {
            var result = ClassificationNodeUtils.RemoveClassificationTypeFromPath(testCase.Key);
            result.Should().Be(testCase.Value, $"because '{testCase.Key}' should transform to '{testCase.Value}'");
        }
    }
}
